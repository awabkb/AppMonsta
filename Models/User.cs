using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public AspCompany AspCompany {get;set;}
        public Boolean IsAdmin { get; set; }
        public DateTime RegisteredAt { get; set; }
        public IEnumerable<SiteVisit> SiteVisits { get; set; }
        
    }
}