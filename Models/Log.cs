using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace IMK_web.Models
{
    public class Log
    {
        [Key]
        public int LogId { get; set; }
        public string Command { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
	//[Column(TypeName = "json")]
        public string Result { get; set; }
    }
}
