using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class Country
    {
	[Key]
        public string Code { get; set; }
        public string Name { get; set; }
        // public string MA { get; set; }
        public IEnumerable<Operator> Operators { get; set; }
        public IEnumerable<AspCompany> AspCompanies { get; set; }
    }
}
