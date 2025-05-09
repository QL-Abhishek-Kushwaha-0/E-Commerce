using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTO.RequestDtos
{
    public class ForgotPasswordDto
    {
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email can't be more than 100 characters!!!")]
        public required string Email { get; set; }
    }
}
