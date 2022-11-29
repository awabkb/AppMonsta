using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Models.EritopAspModels
{
    public class CreateWorkplanModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SiteId { get; set; }
        public string IMKSiteName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public User User { get; set; }
        public string UserEmail { get; set; }
        public string Type { get; set; }
        public AspCompany AspCompany { get; set; }
        public int? AspCompanyId { get; set; }
        public string AspCompanyName { get; set; }
        public string StartDateTime { get; set; }
        public string CompleteDateTime { get; set; }
    }
}
