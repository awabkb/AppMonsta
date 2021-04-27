


using System;
using System.Collections.Generic;
using IMK_web.Models;

namespace Data{
    public class SiteVisitDto{
        public DateTime UploadedAt { get; set; }
        public LogDTO[] Actions { get; set; }
        public string SiteName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Country { get; set; }
        public double RpiVersion { get; set; }
        public double AppVersion { get; set; }

        public SiteVisitDto()
        {
            UploadedAt= DateTime.Now;
        }
    }
}
