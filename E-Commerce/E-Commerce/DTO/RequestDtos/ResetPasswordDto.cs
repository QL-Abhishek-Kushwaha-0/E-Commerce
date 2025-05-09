using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTO.RequestDtos
{
    public class ResetPasswordDto
    {
        [MinLength(8, ErrorMessage ="Password length should be of minimum 8 charaters length!!!")]
        public required string Password { get; set; }
    }
}
