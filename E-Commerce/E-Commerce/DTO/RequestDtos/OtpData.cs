using E_Commerce.Models.Entities;

namespace E_Commerce.DTO.RequestDtos
{
    public class OtpData
    {
        public required int Otp { get; set; }
        public UserRole Role { get; set; }
    }
}
