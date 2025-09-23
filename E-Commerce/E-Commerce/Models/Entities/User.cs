using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models.Entities
{
    public enum UserRole
    {
        Customer,
        Seller
    }
    public class User
    {
        public Guid Id { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(8)]
        public string? Password { get; set; }

        [RegularExpression(@"^\d{10}$")]
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }

        public ICollection<Address>? UserAddresses { get; set; } 
    }
}
