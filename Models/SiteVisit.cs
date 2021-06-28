

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace IMK_web.Models
{
    public class SiteVisit
    {
        [Key]
        public int VisitId { get; set; }
        public User User { get; set; }
        public Site Site{get;set;}
        public IMK_Functions IMK_Functions { get; set; }
        // public DateTime VistedAt { get; set; }
        public IEnumerable<Log> Logs { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public double AppVersion { get; set; }
        public double RPIVersion { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
       
    }
    
}
