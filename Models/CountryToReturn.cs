using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMK_web.Models
{
    public class CountryToReturn
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string[] Operators { get; set; }
        public string[] AspCompanies { get; set; }

    }
}
