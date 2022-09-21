using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Models.EritopAspModels
{
    public class SitesWithWorkplansOutputModel
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public string SiteLatitude { get; set; }
        public string SiteLongitude { get; set; }
        public IEnumerable<Workplans> WorkplanList { get; set; }
    }
}
