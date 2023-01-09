using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace AppMonsta.Models
{
    [NotMapped]
    public class Genre
    {
       public  string Genre_id { get; set; }
       public string Name { get; set; }
    }
}
