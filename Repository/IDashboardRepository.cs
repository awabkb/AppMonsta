using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMK_web.Models;

namespace IMK_web.Repository
{
    public interface IDashboardRepository
    {
        Task<IEnumerable<Site>> GetIMKCountries();
        Task<IEnumerable<Country>> GetOperatorsByCountry(string countries);
        Task<ActionResult> GetSiteVisits(string start, string end);
        Task<ActionResult> GetSitesByCountry(string start, string end);
        Task<ActionResult> GetIMKFunctions(string start, string end);
        Task<ActionResult> GetTopEngineers(string start, string end);
        Task<ActionResult> GetAppVersion(string start, string end);
        Task<ActionResult> GetRPIVersion(string start, string end);
        Task<ActionResult> GetSiteVisitDetails(string start, string end);
    }
}