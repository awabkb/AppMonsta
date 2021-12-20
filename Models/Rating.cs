using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMK_web.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        public int Rate { get; set; }
        public string Questions { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTime? Date { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        [NotMapped]
        public string Country { get; set; }

    }
}