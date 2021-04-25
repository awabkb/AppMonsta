

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
        public DateTime VistedAt { get; set; }
        public ImkVersion ImkVersion { get; set; }
        public IEnumerable<Log> Logs { get; set; }
       
    }
    
}