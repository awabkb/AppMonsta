using System.ComponentModel.DataAnnotations.Schema;

namespace AppMonsta.Models
{
    [NotMapped]
    public class AppDetails
    {
        public string content_rating { get; set; }
        public string app_name { get; set; }
        public bool top_developer { get; set; }
        public string publisher_id_num { get; set; }
        public string requires_os { get; set; }
        public List<string> video_urls { get; set; }
        public string file_size { get; set; }
        public string publisher_name { get; set; }
        public string id { get; set; }
        public string price_currency { get; set; }
        public List<string> genres { get; set; }
        public string app_type { get; set; }
        public string icon_url { get; set; }
        public string content_rating_info { get; set; }
        public string interactive_elements { get; set; }
        public string version { get; set; }
        public string publisher_url { get; set; }
        public bool contains_ads { get; set; }
        public string whats_new { get; set; }
        public string publisher_id { get; set; }
        public List<string> screenshot_urls { get; set; }
        public string status { get; set; }
        public string publisher_email { get; set; }
        public string description { get; set; }
        public double all_rating { get; set; }
        public string genre_id { get; set; }

        public string[] genre_ids { get; set; }


    }
}
