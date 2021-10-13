using System;
using System.Collections.Generic;
using RestSharp;
namespace Data 
{
    public class LogsFilter
    { 
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public string Country { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Command { get; set; }
        public string Result { get; set; }

    }
}
