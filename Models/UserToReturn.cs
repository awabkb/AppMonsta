using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class UserToReturn
    {
        [Key]
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AspCompany {get;set;}
        public Boolean IsAdmin { get; set; }
        public Boolean IsActive { get; set; }
        
    }
}