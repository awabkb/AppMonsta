
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace IMK_web.Models
{
    public class Operator
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
	    [JsonIgnore]
        public IEnumerable<Site> Sites { get; set; }
    }
}
