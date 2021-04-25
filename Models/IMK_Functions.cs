using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class IMK_Functions
    {
        [Key]
        public int Id {get;set;}
        public int VSWR {get;set;}
        public int FRU {get;set;}
        public int CPRI { get; set; }
        public int RSSILTE { get; set; }
        public int RSSIUMTS { get; set; }
        public int RSSINR { get; set; }
        public int IPROUT { get; set; }

        public int IPInterfaces { get; set; }
        
        public int RETAntenna { get; set; }
        public int RetSerial { get; set; }
        public int Alarms { get; set; }
    }
}