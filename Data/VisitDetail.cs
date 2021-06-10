using System;
using System.Collections.Generic;
using IMK_web.Models;

namespace Data{
    public class VisitDetail{
        public string SiteName { get; set; }
        public string Country { get; set; }
        public string User { get; set; }
        public string RpiVersion { get; set; }
        public string AppVersion { get; set; }
        public String ASP { get; set; }
        public String Date { get; set; }
        public bool IsRevisit { get; set; }
        
    }
}
