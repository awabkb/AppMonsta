using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMK_web.Models
{
    public class Log
    {
        [Key]
        public string LogId { get; set; }


        [Column(TypeName = "json")]
        public string Command { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Result { get; set; }
    }
}