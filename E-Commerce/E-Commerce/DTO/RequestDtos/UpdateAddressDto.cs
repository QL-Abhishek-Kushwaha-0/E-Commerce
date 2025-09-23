using E_Commerce.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTO.RequestDtos
{
    public class UpdateAddressDto
    {
        [StringLength(200, ErrorMessage = "Exceeding the Minimum characters for address!!!")]
        public string? AddressLine1 { get; set; }

        [StringLength(200, ErrorMessage = "Exceeding the Minimum characters for address!!!")]
        public string? AddressLine2 { get; set; }

        [StringLength(20, ErrorMessage = "Exceeding the Minimum characters for City!!!")]
        public string? City { get; set; }

        [StringLength(30, ErrorMessage = "Exceeding the Minimum characters for State!!!")]
        public string? State { get; set; }

        [StringLength(50, ErrorMessage = "Exceeding the Minimum characters for Country Name!!!")]
        public string? Country { get; set; } = "India";

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal Code should be of length = 6")]
        public int PostalCode { get; set; }

        public AddressType AddressType { get; set; }
    }
}
