


using System;
using System.Collections.Generic;
using IMK_web.Models;

namespace Data{
    public class SiteVisitDto{

        public int Id { get; set; }
        public LogDTO[] Actions { get; set; }
        public string SiteName { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Country { get; set; }
        public double RpiVersion { get; set; }
        public double AppVersion { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
    }
}
