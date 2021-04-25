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
        [HttpGet("countries")]
        public async Task<List<string>> getIMKCountries()
        {
            var countries = await _dashRepository.GetIMKCountries();
            // return countries.GroupBy(c => c.Country).SelectMany(g =>g).ToList();
            return countries.Select(c => c.Country).Distinct().ToList();
        }

        [HttpGet("operators")]
        public async Task<ActionResult<IEnumerable<Country>>> getOperatorsByCountry([FromQuery] string countries)
        {
            var operators = await _dashRepository.GetOperatorsByCountry(countries);
            return operators.ToList();
        }

        ////////////



        ////// get unique site per day
        [HttpGet("unique_sites")]
        public async Task<ActionResult> getUniqueSiteVisits([FromQuery] string start, [FromQuery] string end)
        {
            var sites = await _dashRepository.GetSiteVisits(start, end);
            return sites;
        }
        

        ////// get # sites per country
        [HttpGet("countryview")]
        public async Task<ActionResult> getSiteByCountry([FromQuery] string start, [FromQuery] string end)
        {
            var sites = await _dashRepository.GetSitesByCountry(start, end);
            return sites;
        }


        ////// get IMK functions count 

        [HttpGet("imkfunctions")]
        public async Task<ActionResult> getIMKFunctions([FromQuery] string start, [FromQuery] string end)
        {
            var visits = await _dashRepository.GetIMKFunctions(start, end);
            return visits;
        }


        ////// get top 10 field engineers
        [HttpGet("topasp")]
        public async Task<ActionResult> getTopEngineers([FromQuery] string start, [FromQuery] string end)
        {
            var asp = await _dashRepository.GetTopEngineers(start, end);
            return asp;
        }


        ///// get app version
        [HttpGet("appversion")]
        public async Task<ActionResult> getAppVersion([FromQuery] string start, [FromQuery] string end)
        {
            var versions = await _dashRepository.GetAppVersion(start, end);
            return versions;
        }


        ///// get Raspberry PI version
        [HttpGet("rpversion")]
        public async Task<ActionResult> getRPVersion([FromQuery] string start, [FromQuery] string end)
        {
            var versions = await _dashRepository.GetRPIVersion(start, end);
            return versions;
        }

        //get all site visits details 
        [HttpGet("site_details")]
        public async Task<ActionResult> getSiteDetails([FromQuery] string start, [FromQuery] string end)
        {
            var data = await _dashRepository.GetSiteVisitDetails(start, end);
            return data;
        }
    }
}