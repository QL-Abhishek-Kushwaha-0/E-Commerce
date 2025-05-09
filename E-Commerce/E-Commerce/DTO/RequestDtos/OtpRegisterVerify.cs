using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTO.RequestDtos
{
    public class OtpRegisterVerify
    {
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid Phone Number!!!")]
        public required string PhoneNumber { get; set; }

        public required int Otp { get; set; }
    }
}
