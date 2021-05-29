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
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; }
        public Operator Operator { get; set; }
        public IEnumerable<SiteVisit> SiteVisits { get; set; }
    }
}
