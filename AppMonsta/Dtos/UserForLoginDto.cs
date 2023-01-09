using System.ComponentModel.DataAnnotations;

namespace AppMonsta.Dtos
{
    public class UserForLoginDto
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
