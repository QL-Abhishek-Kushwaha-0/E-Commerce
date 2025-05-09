namespace E_Commerce.DTO.RequestDtos
{
    public class LoginDto
    {
        public required string Contact {  get; set; }
        public string Password { get; set; } = string.Empty;
        public int Otp { get; set; }
    }
}
