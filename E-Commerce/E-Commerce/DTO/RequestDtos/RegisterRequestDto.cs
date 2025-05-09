using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using E_Commerce.Models.Entities;

namespace E_Commerce.DTO.RequestDtos
{
    public class RegisterRequestDto
    {
        [StringLength(150)]
        public required string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address!!!!")]
        public required string Email { get; set; }

        [MinLength(8)]
        public required string Password { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number should have 10 digits!!")]
        public required string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required UserRole Role { get; set; } = UserRole.Customer;
    }
}
