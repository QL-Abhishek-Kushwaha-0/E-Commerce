using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using E_Commerce.Models.Entities;

namespace E_Commerce.DTO.RequestDtos
{
    public class OtpRegisterRequest
    {
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid Phone Number!!!")]
        public required string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; } = UserRole.Customer;
    }
}
