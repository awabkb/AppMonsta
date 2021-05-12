using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class AspManager
    {
        [Key]
        public int Id { get; set; }
        public AspCompany AspCompany { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

    }
}