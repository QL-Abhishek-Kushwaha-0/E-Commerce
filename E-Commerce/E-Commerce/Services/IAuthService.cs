using E_Commerce.DTO.RequestDtos;
using E_Commerce.DTO.ResponseDtos;

namespace E_Commerce.Services
{
    public interface IAuthService
    {
        public Task<RegisterResponseDto> Register(RegisterRequestDto registerDto);
        public Task<string> OtpGenRegister(OtpRegisterRequest registerDto);
        public Task<LoginResponseDto> OtpVerifyRegister(OtpRegisterVerify registerDto, string deviceId, string userAgent);
        public Task<LoginResponseDto> Login(LoginDto loginDto, string deviceId, string userAgent);
        public Task<string> OtpGenLogin(LoginDto loginDto);
        public Task<LoginResponseDto> OtpVerifyLogin(LoginDto loginDto, string deviceId, string userAgent);
        public Task<TokenResponseDto> RefreshToken(string refreshToken, string deviceId, string userAgent);
        public Task<string> ForgotPassword(ForgotPasswordDto forgotPasswordDto);
        public Task<string> ResetPassword(ResetPasswordDto resetPasswordDto, string token);

        public Task<string> Logout(Guid userId, string deviceId, string userAgent);
    }
}
