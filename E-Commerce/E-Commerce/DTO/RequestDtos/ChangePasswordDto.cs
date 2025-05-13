using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTO.RequestDtos
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }

        [MinLength(8, ErrorMessage = "Password should be of Minimum 8 characters length!!!")]
        public string NewPassword { get; set; }
    }
}
