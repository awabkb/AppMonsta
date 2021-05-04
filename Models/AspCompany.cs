using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class AspCompany
    {
        [Key]
        public int AspId { get; set; }
        public string Name { get; set; }
        //public User ApsMentor { get; set; }
        public Country Country { get; set; }
        public IEnumerable<User> Workers { get; set; }
        public IEnumerable<Site> Sites { get; set; }
    }
}