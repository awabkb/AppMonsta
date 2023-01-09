using System.ComponentModel.DataAnnotations.Schema;

namespace AppMonsta.Models
{
    [NotMapped]
    public class AggregatedRanking
    {
        public string? Country { get; set; }
        public string? Genre_id { get; set; }
        public string? Rank_id { get; set; }
        public List<string>? Ranks { get; set; }
    }
}
