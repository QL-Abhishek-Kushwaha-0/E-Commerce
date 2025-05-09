namespace E_Commerce.DTO.ResponseDtos
{
    public class LoginResponseDto
    {
        public string? AccessToken { get; set; } 
        public string? RefreshToken {  get; set; }
        public string? Message { get; set; } = "Login Success!!!";
    }
}
