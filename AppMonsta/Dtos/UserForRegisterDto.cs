using System.ComponentModel.DataAnnotations;

namespace AppMonsta.Dtos
{
    public class UserForRegisterDto
    {

        [Required]
        public string Email { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "min 5 max 10")]
        public string Password { get; set; }
    }
}
