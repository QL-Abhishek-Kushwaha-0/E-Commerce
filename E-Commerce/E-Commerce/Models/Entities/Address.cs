using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models.Entities
{
    public enum AddressType
    {
        Home,
        Office
    }
    public class Address
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string AddressLine1 { get; set; }

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [StringLength(20)]
        public string City { get; set; }

        [StringLength(30)]
        public string State {  get; set; }

        [StringLength(50)]
        public string Country { get; set; } = "India";

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal Code should be of length = 6")]
        public int PostalCode { get; set; }

        public AddressType AddressType { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
