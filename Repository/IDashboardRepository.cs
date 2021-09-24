using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMK_web.Models;

namespace IMK_web.Repository
{
    public interface IDashboardRepository
    {
        Task<IEnumerable<Site>> GetIMKCountries();
        Task<IEnumerable<Site>> GetIMKCountriesByMA(string MA);
        Task<IEnumerable<Country>> GetOperatorsByCountry(string countries);
        Task<ActionResult> GetSiteVisits(string start, string end, string countries, string operators);
        Task<ActionResult> GetSitesByCountry(string start, string end, string countries, string operators);
        Task<ActionResult> GetSiteRevisits(string start, string end, string countries, string operators);
        Task<ActionResult> GetIMKFunctions(string start, string end, string countries, string operators);
        Task<ActionResult> GetTopEngineers(string start, string end, string countries, string operators);
        Task<ActionResult> GetAppVersion(string start, string end, string countries, string operators);
        Task<ActionResult> GetRPIVersion(string start, string end, string countries, string operators);
        Task<ActionResult> GetSiteVisitDetails(string start, string end, string countries, string operators);
        Task<ActionResult> GetCommandStatus(string start, string end, string countries, string operators);
        Task<ActionResult> GetSiteUsage(string start, string end, string marketArea);
        Task<ActionResult> GetActiveUsers(string start, string end, string marketArea);
        Task<ActionResult> GetNewProfiles(string start, string end, string marketArea);
        Task<ActionResult> GetTopRevisits(string start, string end, string countries, string operators);
        Task<ActionResult> GetResolvedFailures(string start, string end, string countries, string operators);
        string GetRole(string email);





        
    }
}