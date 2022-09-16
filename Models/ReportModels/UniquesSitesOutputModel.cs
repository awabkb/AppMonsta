using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Models.ReportModels
{
    public class UniquesSitesOutputModel
    {
        public List<SiteVisit> VisitsList { get; set; }
        public Dictionary<string, Dictionary<string, int>> VisitDict { get; set; }
    }
}
