using System.CodeDom.Compiler;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blog_Application.Utils;
using E_Commerce.Data;
using E_Commerce.DTO.RequestDtos;
using E_Commerce.DTO.ResponseDtos;
using E_Commerce.Helper;
using E_Commerce.Models.Entities;
using E_Commerce.Resources;
using E_Commerce.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace E_Commerce.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;
        private readonly IConnectionMultiplexer _redis;
        private readonly EmailService _emailService;
        public AuthService(ApplicationDbContext context, IJwtService jwtService, IConfiguration config, IConnectionMultiplexer redis, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _config = config;
            _redis = redis;
            _emailService = emailService;
        }

        // Registering the User using Email and Password
        public async Task<RegisterResponseDto> Register(RegisterRequestDto registerRequestDto)
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == registerRequestDto.Email || u.PhoneNumber == registerRequestDto.PhoneNumber);

            if (existingUser) return null;

            var newUser = new User
            {
                Name = registerRequestDto.Name,
                Email = registerRequestDto.Email,
                Password = PasswordHasher.HashPassword(registerRequestDto.Password),
                PhoneNumber = registerRequestDto.PhoneNumber,
                Role = registerRequestDto.Role,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await _emailService.SendMail(newUser.Email, newUser.Name, EmailMessages.RegisterSubject, EmailMessages.RegisterSuccessEmail());

            return new RegisterResponseDto { Name = newUser.Name, Email = newUser.Email, PhoneNumber = newUser.PhoneNumber, Role = newUser.Role };
        }

        // Generating OTP for Registering a new user
        public async Task<string> OtpGenRegister(OtpRegisterRequest registerDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerDto.PhoneNumber);

            if (user != null) return ResponseMessages.ALREADY_EXISTS;

            var otp = HelperFunctions.GenerateOtp();    // Generating the OTP (4 digits)

            // Storing the OTP in Redis DB for 1 minute
            var redisDb = _redis.GetDatabase();
            var dataKey = $"phone:{registerDto.PhoneNumber}";

            var data = new OtpData
            {
                Otp = otp,
                Role = registerDto.Role
            };

            var jsonData = JsonConvert.SerializeObject(data);       // Need to Serialize as Value in DB cannot be a object

            await redisDb.StringSetAsync(dataKey, jsonData, TimeSpan.FromSeconds(60));

            //Console.WriteLine("OTP : {0}", otp);

            //HelperFunctions.SendOtp(otp, _config);        // -> For sending OTP to user's phone using Twilio 

            return ResponseMessages.SUCCESS;
        }

        // Verifying the OTP for Registering a new user
        public async Task<LoginResponseDto> OtpVerifyRegister(OtpRegisterVerify registerDto, string deviceId, string userAgent)
        {
            // Fetch the OtpData from Redis DB using PhoneNumber
            var redisDb = _redis.GetDatabase();
            var dataKey = $"phone:{registerDto.PhoneNumber}";       // Format of data key to search on Redis

            var data = await redisDb.StringGetAsync(dataKey);

            if (data.IsNullOrEmpty) return null;

            var jsonData = JsonConvert.DeserializeObject<OtpData>(data!);   // Deserialize the data fetched from Redis

            if (jsonData == null) return null;

            // Check if OTP matches the user's enteredOTP or else check with the static OTP for testing purpose
            if (jsonData.Otp == registerDto.Otp || registerDto.Otp == HelperFunctions.GetStaticOtp(_config))
            {
                // Create a User Entry in the DB
                var newUser = new User
                {
                    PhoneNumber = registerDto.PhoneNumber,
                    Role = jsonData.Role,
                };

                _context.Users.Add(newUser); // Add new User to Users

                await _context.SaveChangesAsync();

                // For Registering through OTP -> we'll directly Sign in the User

                // Fetch the User Created
                var currUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerDto.PhoneNumber);

                if (currUser == null) return null;

                // Generate Access and Refresh Token
                var res = new LoginResponseDto
                {
                    AccessToken = _jwtService.GenerateJwtToken(currUser, "Access"),
                    RefreshToken = _jwtService.GenerateJwtToken(currUser, "Refresh")
                };

                // Create a Refresh Token Entry in the DB along with device Id as well as user agent
                await SaveRefreshToken(currUser.Id, res.RefreshToken, deviceId, userAgent);

                return res;
            }

            return null;
        }

        // Login using Email and Password
        public async Task<LoginResponseDto> Login(LoginDto loginDto, string deviceId, string userAgent)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Contact);

            if (user == null)
                return null;

            // In case when user registered using Phone number and updated user details by adding Email but not the Password
            if (user.Password == null)
                return new LoginResponseDto { Message = ResponseMessages.PASSWORD_NOT_SET };

            if (!PasswordHasher.VerifyPassword(loginDto.Password, user.Password))
                return new LoginResponseDto { Message = ResponseMessages.WRONG_PASSWORD };

            // Generate Access and Refresh Token
            string accessToken = _jwtService.GenerateJwtToken(user, "Access");
            string refreshToken = _jwtService.GenerateJwtToken(user, "Refresh");

            // Create a Refresh Token Entry in the DB along with device Id as well as user agent
            await SaveRefreshToken(user.Id, refreshToken, deviceId, userAgent);

            return new LoginResponseDto { AccessToken = accessToken, RefreshToken = refreshToken, Message = ResponseMessages.SUCCESS };
        }

        // Generating OTP for Login using Phone Number
        public async Task<string?> OtpGenLogin(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.Contact);

            if (user == null) return null;

            // Storing the OTP in Redis DB for 1 minute
            var redisDb = _redis.GetDatabase();
            var dataKey = $"phone:{loginDto.Contact}";

            var data = new OtpData
            {
                Otp = HelperFunctions.GenerateOtp(),
                Role = user.Role
            };

            //HelperFunctions.SendOtp(data.Otp, _config);      // -> For sending OTP to user's phone from Twilio

            var dataValue = JsonConvert.SerializeObject(data);
            await redisDb.StringSetAsync(dataKey, dataValue, TimeSpan.FromSeconds(60));

            Console.WriteLine(data.Otp);

            return ResponseMessages.SUCCESS;
        }

        // Verifying the OTP for Login using Phone Number
        public async Task<LoginResponseDto> OtpVerifyLogin(LoginDto loginDto, string deviceId, string userAgent)
        {
            // Fetch the OtpData from Redis DB using PhoneNumber
            var redisDb = _redis.GetDatabase();
            var dataKey = $"phone:{loginDto.Contact}";

            var data = await redisDb.StringGetAsync(dataKey);
            if (data.IsNullOrEmpty) return null;

            var otpData = JsonConvert.DeserializeObject<OtpData>(data);
            if (otpData == null) return null;

            // Check if OTP matches the user's enteredOTP or else check with the static OTP for testing purpose
            if (otpData.Otp == loginDto.Otp || loginDto.Otp == HelperFunctions.GetStaticOtp(_config))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.Contact);
                if (user == null) return null;

                var response = new LoginResponseDto
                {
                    AccessToken = _jwtService.GenerateJwtToken(user, "Access"),
                    RefreshToken = _jwtService.GenerateJwtToken(user, "Refresh")
                };

                // Create a Refresh Token Entry in the DB along with device Id as well as user agent
                await SaveRefreshToken(user.Id, response.RefreshToken, deviceId, userAgent);

                return response;
            }

            return null;
        }

        // Refreshing the Access Token using Refresh Token
        public async Task<TokenResponseDto> RefreshToken(string refreshToken, string deviceId, string userAgent)
        {
            // Check if the refresh token is valid
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(refreshToken))
                return null;

            // Extract the UserId from the refresh token
            var jwtToken = tokenHandler.ReadJwtToken(refreshToken);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // Parse the UserId from string to Guid
            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return null;

            // Fetch the User from the DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // Fetch the Refresh Token from the DB
            var refreshTokenRecord = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.RefreshToken == refreshToken && rt.DeviceId == deviceId && rt.UserAgent == userAgent);

            // Check for matching refresh token as well as its validity
            if (user == null || refreshTokenRecord == null || jwtToken.ValidTo < DateTime.UtcNow.ToLocalTime())
            {
                return null;
            }

            // Generate new Tokens
            var newAccessToken = _jwtService.GenerateJwtToken(user, "Access");
            var newRefreshToken = _jwtService.GenerateJwtToken(user, "Refresh");

            // Update the Refresh token in the User's table in DB
            refreshTokenRecord.RefreshToken = newRefreshToken;
            refreshTokenRecord.ExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }


        // Sending the Forgot Password link to the user's email
        public async Task<string> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);

            if (user == null) return ResponseMessages.INVALID_EMAIL;

            // Generate a password reset link for the user
            var url = $"https://localhost:7290/api/auth/reset-password?token={user.Password}";

            // Storing the token in Redis for 10 minutes with Email as value
            var redisDb = _redis.GetDatabase();
            var dataKey = $"token:{user.Password}";

            await redisDb.StringSetAsync(dataKey, user.Email, TimeSpan.FromMinutes(10));

            // Send the password reset link to the user's email
            await _emailService.SendMail(user.Email, user.Name, EmailMessages.ForgotPasswordSubject, EmailMessages.ForgotPasswordEmail(url));

            return ResponseMessages.SUCCESS;
        }

        // Resetting the Password using the token
        public async Task<string> ResetPassword(ResetPasswordDto resetPasswordDto, string token)
        {
            // Fetch the token from Redis using the token
            var db = _redis.GetDatabase();
            var dataKey = $"token:{token}";     // Format of data key to search on Redis

            // Fetched the Email from the Redis using the token
            var userEmail = await db.StringGetAsync(dataKey);

            if (userEmail.IsNullOrEmpty) return "InvalidUser";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail.ToString());      // Need to convert userEmail to string as it is of form RedisValue

            if (user == null) return "InvalidUser";

            // Updates the Password in the DB
            user.Password = PasswordHasher.HashPassword(resetPasswordDto.Password);

            await _context.SaveChangesAsync();

            // Deleted the record from Redis
            await db.KeyDeleteAsync(dataKey);

            return "Success";
        }

        // Logging out the user
        public async Task<string> Logout(Guid userId, string deviceId, string userAgent)
        {
            // Fetch the Refresh Token from the DB
            var refreshTokenRecord = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.DeviceId == deviceId && rt.UserAgent == userAgent);

            if (refreshTokenRecord == null) return null;

            // Delete the Refresh Token from the DB
            _context.RefreshTokens.Remove(refreshTokenRecord);
            await _context.SaveChangesAsync();

            return ResponseMessages.SUCCESS;
        }

        // Saving the Refresh Token in the DB
        private async Task SaveRefreshToken(Guid userId, string refreshToken, string deviceId, string userAgent)
        {
            var refreshTokenRecord = new Refreshtoken
            {
                UserId = userId,
                RefreshToken = refreshToken,
                DeviceId = deviceId,
                UserAgent = userAgent
            };

            _context.RefreshTokens.Add(refreshTokenRecord);

            await _context.SaveChangesAsync(); // Save the Changes in the DB
        }
    }
}

