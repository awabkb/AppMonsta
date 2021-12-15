using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Data;
using IMK_web.Models;
using IMK_web.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace IMK_web.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]

    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : Controller
    {
        private readonly IDashboardRepository _dashRepository;

        public DashboardApiController(IDashboardRepository dashboardRepository)
        {
            _dashRepository = dashboardRepository;
        }

        ////// Filtering //////
        [Authorize]
        // [HttpGet("countries")]
        // public async Task<List<string>> getIMKCountries()
        // {
        //     var countries = await _dashRepository.GetIMKCountries();
        //     // return countries.GroupBy(c => c.Country).SelectMany(g =>g).ToList();
        //     return countries.Select(c => c.Country).Distinct().ToList();
        // }

        [HttpGet("countries")]
        public async Task<List<string>> getIMKCountriesByMA([FromQuery] string marketArea)
        {
            var countries = await _dashRepository.GetIMKCountriesByMA(marketArea);
            return countries.Select(c => c.Country).Distinct().ToList();
        }

        [HttpGet("operators")]
        public async Task<ActionResult<IEnumerable<Country>>> getOperatorsByCountry([FromQuery] string countries)
        {
            if (countries == null)
                return null;
            else
            {
                var operators = await _dashRepository.GetOperatorsByCountry(countries);
                return operators.ToList();
            }
        }

        ////////////


        [AllowAnonymous]
        ////// get unique site per day
        [HttpGet("unique_sites")]
        public async Task<ActionResult> getUniqueSiteVisits([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var sites = await _dashRepository.GetSiteVisits(start, end, countries, operators);
            return sites;
        }

        [AllowAnonymous]
        ////// get # sites per country
        [HttpGet("countryview")]
        public async Task<ActionResult> getSiteByCountry([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var sites = await _dashRepository.GetSitesByCountry(start, end, countries, operators);
            return sites;
        }


        [AllowAnonymous]
        ////// get # site revisits
        [HttpGet("revisits")]
        public async Task<ActionResult> getSiteRevisits([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var revisits = await _dashRepository.GetSiteRevisits(start, end, countries, operators);
            return revisits;
        }

        ////// get IMK functions count 
        [AllowAnonymous]
        [HttpGet("imkfunctions")]
        public async Task<ActionResult> getIMKFunctions([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var visits = await _dashRepository.GetIMKFunctions(start, end, countries, operators);
            return visits;
        }

        [AllowAnonymous]
        ////// get top 10 field engineers
        [HttpGet("topasp")]
        public async Task<ActionResult> getTopEngineers([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var asp = await _dashRepository.GetTopEngineers(start, end, countries, operators);
            return asp;
        }


        ///// get app version
        [HttpGet("appversion")]
        public async Task<ActionResult> getAppVersion([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var versions = await _dashRepository.GetAppVersion(start, end, countries, operators);
            return versions;
        }


        ///// get Raspberry PI version
        [HttpGet("rpversion")]
        public async Task<ActionResult> getRPVersion([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var versions = await _dashRepository.GetRPIVersion(start, end, countries, operators);
            return versions;
        }

        //get all site visits details 
        [HttpGet("site_details")]
        public async Task<ActionResult> getSiteDetails([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var data = await _dashRepository.GetSiteVisitDetails(start, end, countries, operators);
            return data;
        }

        //get all site visits details new
        [HttpGet("site_details_new")]
        public async Task<ActionResult> getSiteDetailsNew([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var data = await _dashRepository.GetSiteVisitDetailsNew(start, end, countries, operators);
            return data;
        }

        [HttpGet("commands")]
        public async Task<ActionResult[]> getCommandStatus([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            ActionResult[] data = new ActionResult[2];
            data[0] = await _dashRepository.GetCommandStatus(start, end, countries, operators);
            data[1] = await _dashRepository.GetResolvedFailures(start, end, countries, operators);

            return data;
        }

        //get top 10 revisits by site name
        [HttpGet("top-revisits")]
        public async Task<ActionResult> getTopRevisits([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var data = await _dashRepository.GetTopRevisits(start, end, countries, operators);
            return data;
        }

        //get number of resolved failures
        [HttpGet("resolved")]
        public async Task<ActionResult> getResolvedFailures([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var data = await _dashRepository.GetResolvedFailures(start, end, countries, operators);
            return data;
        }

        [HttpGet("site_integrations")]
        public async Task<ActionResult> getSiteIntegrations([FromQuery] string start, [FromQuery] string end, [FromQuery] string countries, [FromQuery] string operators)
        {
            var data = await _dashRepository.GetSiteIntegrations(start, end, countries, operators);
            return data;
        }              
                
        [HttpGet("alarms")]
        public async Task<ActionResult> getAlarms() {
            var alarms = await _dashRepository.GetAlarmTypes();
            return alarms;
        }

        [HttpGet("claims")]
        public List<System.Security.Claims.Claim> getSignum(){
            var signum = User.Claims.ToList();
            return signum;
        }
        




        //////////////////////////////////////////////////////////////////////////////////////////////
        //get # of site usage 
        [HttpGet("usage")]
        public async Task<ActionResult> getSiteUsage([FromQuery] string start, [FromQuery] string end, [FromQuery] string marketArea)
        {
            var data = await _dashRepository.GetSiteUsage(start, end, marketArea);
            return data;
        }

        //get # of active users 
        [HttpGet("active_users")]
        public async Task<ActionResult> getActiveUsers([FromQuery] string start, [FromQuery] string end, [FromQuery] string marketArea)
        {
            var data = await _dashRepository.GetActiveUsers(start, end, marketArea);
            return data;
        }

        //get # of new users 
        [HttpGet("new_users")]
        public async Task<ActionResult> getNewProfiles([FromQuery] string start, [FromQuery] string end, [FromQuery] string marketArea)
        {
            var data = await _dashRepository.GetNewProfiles(start, end, marketArea);
            return data;
        }
        
        [HttpGet("lmt-usage")]
        public async Task<ActionResult> getSiteIntegrationsUsage([FromQuery] string start, [FromQuery] string end, [FromQuery] string marketArea)
        {
            var data = await _dashRepository.GetSiteIntegrationUsage(start, end, marketArea);
            return data;
        }

    }
}