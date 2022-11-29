using IMK_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class TransportNodeDTO
    {
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
