using System;
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
        public string DownloadEnd { get; set; }
        public string IntegrateStart { get; set; }
        public string IntegrateEnd { get; set; }
        public string Outcome { get; set; }
        public bool Downloading { get; set; }
        public bool Integrating { get; set; }
        public string UserId { get; set; }
        public string MacAddress { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string AppVersion { get; set; }
        public string Error { get; set; }
        public int Progress { get; set; }
        //public byte[] AiLog { get; set; }
        public string InitiatedAt { get; set; }
        public DateTime DownloadStartDatetime {get;set;}
        public DateTime DownloadEndDatetime {get;set;}
        public DateTime IntegrateStartDatetime {get;set;}
        public DateTime IntegrateEndDatetime {get;set;}

    }
}
