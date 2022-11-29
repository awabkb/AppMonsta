using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Models
{
    public class TransportNode
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string CountryName { get; set; }
        public DateTime ConfigurationDate { get; set; }
        public string Status { get; set; }
        public string Commands { get; set; }
        public string Comments { get; set; }
        public string MacAddress { get; set; }

    }
}
