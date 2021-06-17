using System;
using System.Collections.Generic;
using RestSharp;
namespace Data 
{
    public class LogDTO
    {
        public int LogId { get; set; }
        public string Command { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public JsonObject[] Result { get; set; }
        public long ResponseTime { get; set; }

    }
}
