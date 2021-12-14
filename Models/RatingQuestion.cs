using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class RatingQuestion
    {
        [Key]
        public int Id { get; set; }
        public string Question { get; set; }
        
    }
}