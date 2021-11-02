using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class AspManager
    {
        [Key]
        public int Id { get; set; }
        public string Country { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }


    }
}