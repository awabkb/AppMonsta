using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Models.EritopAspModels
{
    public class EritopSitesOutputModel
    {
        public List<SitesWithWorkplansOutputModel> Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}
