using System.Text.Json.Serialization;
using E_Commerce.Models.Entities;

namespace E_Commerce.DTO.ResponseDtos
{
    public class RegisterResponseDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }
    }
}
