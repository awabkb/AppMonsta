using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class SiteIntegration
    {
        [Key]
        public int Id { get; set; }
        public string SiteName { get; set; }
        public string DownloadStart { get; set; }
        public string DownloadEnd  { get; set; }
        public string IntegrateStart { get; set; }
        public string IntegrateEnd { get; set; }
        public string Outcome { get; set; }
        public bool Downloading { get; set; }
        public bool Integrating { get; set; }
        public string UserId { get; set; }
        public string MACAddress { get; set; }
    }
}
