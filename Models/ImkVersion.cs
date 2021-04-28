using System;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class ImkVersion
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateOfRelease { get; set; }
        public double AppVersion { get; set; }
        public double RPIVersion { get; set; }
        
    }
}