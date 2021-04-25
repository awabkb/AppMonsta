using System.Collections;
using System.Collections.Generic;
using System;

namespace Data{
    public class ActionDTO{
        public string Command { get; set; }
        public string SiteUUID { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Country { get; set; }

        public string SiteName { get; set; }

        public DateTime UploadedAt { get; set; }

        public string Result { get; set; }
    }
}