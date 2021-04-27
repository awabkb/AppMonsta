using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class Site
    {
        [Key]
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string longitude { get; set; }
        public string Country { get; set; }
        public Operator Operator { get; set; }
        public AspCompany AspCompany { get; set; }
        public IEnumerable<SiteVisit> SiteVisits { get; set; }
    }
}
