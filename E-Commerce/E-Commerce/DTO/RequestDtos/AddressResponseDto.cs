using System.Text.Json.Serialization;
using E_Commerce.Models.Entities;

namespace E_Commerce.DTO.RequestDtos
{
    public class AddressResponseDto
    {
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AddressType AddressType { get; set; }
    }
}
