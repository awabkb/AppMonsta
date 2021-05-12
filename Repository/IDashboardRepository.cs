using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMK_web.Models;

namespace IMK_web.Repository
{
    public interface IDashboardRepository
    {
        void Add<T>(T entity) where T: class;
        void Update<T>(T entity) where T: class;
        Task<bool> SaveChanges();
        Task<IEnumerable<Site>> GetIMKCountries();
        Task<IEnumerable<Country>> GetOperatorsByCountry(string countries);
        Task<ActionResult> GetSiteVisits(string start, string end, string countries, string operators);
        Task<ActionResult> GetSitesByCountry(string start, string end, string countries, string operators);
        Task<ActionResult> GetSiteRevisits(string start, string end, string countries, string operators);
        Task<ActionResult> GetIMKFunctions(string start, string end, string countries, string operators);
        Task<ActionResult> GetTopEngineers(string start, string end, string countries, string operators);
        Task<ActionResult> GetAppVersion(string start, string end, string countries, string operators);
        Task<ActionResult> GetRPIVersion(string start, string end, string countries, string operators);
        Task<ActionResult> GetSiteVisitDetails(string start, string end, string countries, string operators);
        Task<ActionResult> GetSiteUsage(string start, string end);
        Task<ActionResult> GetActiveUsers(string start, string end);
        Task<ActionResult> GetNewProfiles(string start, string end);



        
    }
}