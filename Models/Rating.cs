using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        
    }
}