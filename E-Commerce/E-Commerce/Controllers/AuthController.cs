using System.Security.Claims;
using E_Commerce.Data;
using E_Commerce.DTO.RequestDtos;
using E_Commerce.Helper;
using E_Commerce.Resources;
using E_Commerce.Services;
using E_Commerce.Utils;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // API to Register a new user
        /*
            <summary>
                Register a new user
            </summary>
            <param name="registerRequestDto">User details for registering on Application</param>
            <returns>Returns a success message and the registered user details</returns>
         */
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register(RegisterRequestDto registerRequestDto)
        {
            var user = await _authService.Register(registerRequestDto);

            if(user == null) return BadRequest(new ApiResponse(false, 400, ResponseMessages.USER_EXISTS));

            return Ok(new ApiResponse(true, 200, ResponseMessages.USER_REGISTERED, user));
        }


        // API to Genrerate Otp to Register a new user
        /*
            <summary>
                Generate the Otp on Phone Number of new user
            </summary>
            <param name="otpRequestDto">User details (Phone Number and Role) for registering on Application</param>
            <returns>Returns a success message of Otp Sent</returns>
         */
        [HttpPost("register/generate-otp")]
        public async Task<ActionResult<ApiResponse>> GenOtp(OtpRegisterRequest otpRequest)
        {
            var res = await _authService.OtpGenRegister(otpRequest);

            if (res.Equals(ResponseMessages.ALREADY_EXISTS)) return Conflict(new ApiResponse(false, 409, ResponseMessages.USER_EXISTS));

            return Ok(new ApiResponse(true, 200, "Otp Sent Successfully..."));
        }


        // API to Verify Otp to Register a new user and Log in after otp verification
        /*
            <summary>
                Verify the Otp on Phone Number of new user
            </summary>
            <param name="otpRequest">User details (Phone Number and Role) for registering on Application</param>
            <returns>Returns a success message of Successfull Login and refresh as well as access token</returns>
            <remarks>
                    <para>This API is used to verify the OTP sent to the user's phone number during registration.</para>
                    <para>After successful verification, the user will be logged in and receive access and refresh tokens.</para>
            </remarks>
         */
        [HttpPost("register/verify-otp")]
        public async Task<ActionResult<ApiResponse>> VerifyOtp(OtpRegisterVerify otpRequest)
        {
            // Fetch the deviceId and userAgent from the request headers
            var deviceId = Request.Headers["deviceId"].ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();
            
            if (string.IsNullOrEmpty(deviceId)) return BadRequest(new ApiResponse(false, 400, "Invalid Device!!!"));

            var res = await _authService.OtpVerifyRegister(otpRequest, deviceId, userAgent);
                
            if (res == null) return BadRequest(new ApiResponse(false, 400, "Invalid OTP!!!"));

            return Ok(new ApiResponse(true, 200, "Login Successfull..", res));
        }


        // API to Login using Email and Password
        /*
            <summary>
                Log In for user
            </summary>
            <param name="loginDto">User details for logging in Application</param>
            <returns>Returns a success message and access token as well as refresh token</returns>
         */
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login(LoginDto loginDto)
        {
            if(!HelperFunctions.IsValidEmail(loginDto.Contact)) return BadRequest(new ApiResponse(false, 400, ResponseMessages.INVALID_EMAIL));

            // Fetch the deviceId and userAgent from the request headers
            var deviceId = Request.Headers["deviceId"].ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(deviceId)) return BadRequest(new ApiResponse(false, 400, "Invalid Device!!!"));

            var res = await _authService.Login(loginDto, deviceId, userAgent);

            if(res == null) return BadRequest(new ApiResponse(false, 400, ResponseMessages.NO_USER));
            if (res.Message == ResponseMessages.PASSWORD_NOT_SET) return BadRequest(new ApiResponse(false, 400, ResponseMessages.PASSWORD_NOT_SET));
            if(res.Message == ResponseMessages.WRONG_PASSWORD) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.WRONG_PASSWORD));

            return Ok(new ApiResponse(true, 200, "Login Successfull...", res));
        }


        // API to Generate Otp for Phone OTP Login
        /*
            <summary>
                Generate OTP for Logging In for user
            </summary>
            <param name="loginDto">User details for generating OTP on Phone Number for logging in Application</param>
            <returns>Returns a success message of OTP sent to Phone Number</returns>
         */
        [HttpPost("login/generate-otp")]
        public async Task<ActionResult<ApiResponse>> GenOtpLogin(LoginDto loginDto)
        {
            if(!HelperFunctions.IsValidPhone(loginDto.Contact)) return BadRequest(new ApiResponse(false, 400, "Invalid Phone Number!!!"));

            var result = await _authService.OtpGenLogin(loginDto);

            if (result == null) return BadRequest(new ApiResponse(false, 400, ResponseMessages.NO_USER));

            return Ok(new ApiResponse(true, 200, "Otp Sent Successfully!!!"));
        }


        // API to verify Otp for Phone OTP Login
        /*
            <summary>
                Verify OTP for Logging In for user
            </summary>
            <param name="loginDto">User details for verifying OTP on Phone Number for logging in Application</param>
            <returns>Returns a success message of Successfull Login and refresh as well as access token</returns>
         */
        [HttpPost("login/verify-otp")]
        public async Task<ActionResult<ApiResponse>> VerifyOtpLogin(LoginDto loginDto)
        {
            if (!HelperFunctions.IsValidPhone(loginDto.Contact)) return BadRequest(new ApiResponse(false, 400, "Invalid Phone Number!!!!"));

            // Fetch the deviceId and userAgent from the request headers
            var deviceId = Request.Headers["deviceId"].ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(deviceId)) return BadRequest(new ApiResponse(false, 400, "Invalid Device!!!"));

            var res = await _authService.OtpVerifyLogin(loginDto, deviceId, userAgent);

            if (res == null) return BadRequest(new ApiResponse(false, 400, "Invalid OTP"));

            return Ok(new ApiResponse(true, 200, "Login Successfull", res));
        }

        // API to Refresh the Access Token
        /*
            <summary>
                Refresh the Access Token as well as Refresh token
            </summary>
            <param name="refreshToken through header">Refresh Token</param>
            <returns>Returns a success message and new access token and Refresh Token</returns>
         */
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse>> RefreshToken()
        {
            // Fetch the refresh token, deviceId, userAgent from the request headers
            var refreshToken = Request.Headers["refresh-token"].ToString();
            var deviceId = Request.Headers["deviceId"].ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(refreshToken)) return BadRequest(new ApiResponse(false, 400, "Invalid Refresh Token!!!"));
            if (string.IsNullOrEmpty(deviceId)) return BadRequest(new ApiResponse(false, 400, "Invalid Device!!!"));

            var res = await _authService.RefreshToken(refreshToken, deviceId, userAgent);

            if (res == null) return BadRequest(new ApiResponse(false, 400, "Invalid Refresh Token!!!"));

            return Ok(new ApiResponse(true, 200, "Token Refreshed!!!", res));
        }

        // API to send the password reset link to the user
        /*
            <summary>
                Send the password reset link to the user
            </summary>
            <param name="forgotPasswordDto">User details for sending the password reset link</param>
            <returns>Returns a success message of Password reset link sent</returns>
         */
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse>> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var res = await _authService.ForgotPassword(forgotPasswordDto);

            if (res == ResponseMessages.INVALID_EMAIL) return BadRequest(new ApiResponse(false, 400, ResponseMessages.NO_USER));

            return Ok(new ApiResponse(true, 200, "Password reset link is sent successfullly!!!"));
        }

        // API to reset the password
        /*
            <summary>
                Reset the password of the user
            </summary>
            <param name="resetPasswordDto">User details for resetting the password</param>
            <param name="token">Token for resetting the password</param>
            <returns>Returns a success message of Password reset successfull</returns>
         */
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordDto resetPasswordDto, [FromQuery] string token)
        {
            var response = await _authService.ResetPassword(resetPasswordDto, token);

            if (response.Equals("InvalidUser")) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.INVALID_EMAIL));

            return Ok(new ApiResponse(true, 200, "Password reset Successfull..."));
        }

        // API to Logout the user
        /*
            <summary>
                Logout the user
            </summary>
            <returns>Returns a success message of Logout successfull</returns>
         */
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            // Fetch the deviceId and userAgent from the request headers
            var deviceId = Request.Headers["deviceId"].ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(deviceId)) return BadRequest(new ApiResponse(false, 400, "Invalid Device!!!"));

            // Fetch the userId from the claims
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, "Invalid User!!"));

            var res = await _authService.Logout(userId, deviceId, userAgent);

            if (res == null) return BadRequest(new ApiResponse(false, 400, "Invalid User"));

            return Ok(new ApiResponse(true, 200, "Logout Successfull..."));
        }
    }
}
