using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Models;
using System.Linq;
using IMK_web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;

namespace IMK_web.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DataContext _context;

        public DashboardRepository(DataContext dataContext)
        {
            this._context = dataContext;

        }
        public async Task<IEnumerable<Country>> GetCountries()
        {
            //return await _context.Countries.Include(x=>x.Operators).ToListAsync();
            return await _context.Countries.ToListAsync();
        }

        public async Task<IEnumerable<Operator>> GetOperators()
        {
            return await _context.Operators.ToListAsync();

        }

        public async Task<IEnumerable<Site>> GetOperatorSites(int operatorId)
        {
            var op = await _context.Operators.FirstOrDefaultAsync(x => x.Id == operatorId);
            return op.Sites.ToList();
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // Filter

        //get list of countries that are using IMK from site visits
        public async Task<IEnumerable<Site>> GetIMKCountries()
        {
            return await _context.Sites.OrderBy(s => s.Country).ToListAsync();
        }

        //get operators for countries
        public async Task<IEnumerable<Country>> GetOperatorsByCountry(string countries)
        {
            string[] arrCountries = countries.Split(",");
            return await _context.Countries.Where(c => arrCountries.Contains(c.Name)).Include(x => x.Operators).ToListAsync();
        }


        //get site visits in date range (start -> end) of unique sites
        public async Task<ActionResult> GetSiteVisits(string start, string end)
        {
            var visits = await _context.SiteVisits.OrderBy(s => s.VistedAt).Include("Site").ToListAsync();
            var res = visits.GroupBy(x => x.VistedAt);
            Dictionary<string, int> countrySites = new Dictionary<string, int>();
            foreach (var v in res)
            {
                List<int> ids = new List<int>();
                var date = "";
                foreach (var s in v)
                {
                    date = s.VistedAt.ToString("yyyy-MM-dd");
                    ids.Add(int.Parse(s.Site.SiteId));
                }
                countrySites[date] = ids.Distinct().Count();
            }
            return new JsonResult(countrySites);
        }

        public async Task<ActionResult> GetSitesByCountry(string start, string end)
        {
            var visits = await _context.SiteVisits.OrderBy(s => s.VistedAt).Include("Site").ToListAsync();
            var res = visits.GroupBy(x => x.VistedAt);
            Dictionary<string, Dictionary<string, int>> countryVisits = new Dictionary<string, Dictionary<string, int>>();

            foreach (var v in res)
            {
                Dictionary<string, int> cc = new Dictionary<string, int>();
                var date = "";
                foreach (var s in v)
                {
                    date = s.VistedAt.ToString("yyyy-MM-dd");
                    var count = 1;
                    if (cc.ContainsKey(s.Site.Country))
                    {
                        count++;
                        cc[s.Site.Country] = count;
                    }
                    else
                    {
                        cc.Add(s.Site.Country, count);
                    }
                }
                countryVisits[date] = cc;
            }

            return new JsonResult(countryVisits);
        }


        // get IMK functions used
        public async Task<ActionResult> GetIMKFunctions(string start, string end)
        {
            var sfunctions = await _context.SiteVisits.Include("IMK_Functions").Select(s => s.IMK_Functions.Id).ToListAsync();

            var functions = _context.IMK_Functions.Where(x => sfunctions.Contains(x.Id)).GroupBy(x => true).Select(x => new
            {
                VSWR = x.Sum(y => y.VSWR),
                FRU = x.Sum(y => y.FRU),
                CPRI = x.Sum(y => y.CPRI),
                IPROUT = x.Sum(y => y.IPROUT),
                RetSerial = x.Sum(y => y.RetSerial),
                RSSILTE = x.Sum(y => y.RSSILTE),
                RSSIUMTS = x.Sum(y => y.RSSIUMTS),
                RSSINR = x.Sum(y => y.RSSINR),
                IPInterfaces = x.Sum(y => y.IPInterfaces),
                RETAntenna = x.Sum(y => y.RETAntenna),
                Alarms = x.Sum(y => y.Alarms)
            });
            return new JsonResult(functions);
        }

        // get top 10 engineers
        public async Task<ActionResult> GetTopEngineers(string start, string end)
        {
            var allVisits = await _context.SiteVisits.Include("Site").Include("User").ToListAsync();
            var asp = allVisits.AsEnumerable().GroupBy(x => x.User.Name).Select(y => new { name = y.Key, sites = y.Select(i => i.Site.SiteId).Distinct().Count() });
            var topasp = asp.OrderByDescending(s => s.sites).Take(10);

            return new JsonResult(topasp);
        }

        //get IMKVersion (AppVersion %)

        public async Task<ActionResult> GetAppVersion(string start, string end)
        {
            var allVisits = await _context.SiteVisits.Include("ImkVersion").ToListAsync();
            var versions = allVisits.GroupBy(x => new { x.ImkVersion.AppVersion }).Select(y => new
            {
                AppVersion = y.Key.AppVersion,
                usage = ((float)y.Select(i => i.VisitId).Count() / (float)allVisits.Count()) * 100
            });

            return new JsonResult(versions);
        }


        //get IMKVersion (RPIVersion %)

        public async Task<ActionResult> GetRPIVersion(string start, string end)
        {
            var allVisits = await _context.SiteVisits.Include("ImkVersion").ToListAsync();
            var versions = allVisits.GroupBy(x => new { x.ImkVersion.RPIVersion }).Select(y => new
            {
                RPIVersion = y.Key.RPIVersion,
                usage = ((float)y.Select(i => i.VisitId).Count() / (float)allVisits.Count()) * 100
            });

            return new JsonResult(versions);
        }

        
        // get all site visit details
        public async Task<ActionResult> GetSiteVisitDetails(string start, string end) 
        {
            var allVisits =  _context.SiteVisits.Include("ImkVersion").Include("Site").Include(x => x.Site.AspCompany).Include(x =>x.Site.Operator);
            var visitDetails = await allVisits.OrderByDescending(y => y.VistedAt).Select(x => new {
                siteName = x.Site.Name,
                country = x.Site.Country,
                user = x.User.Name,
                op = x.Site.Operator.Name,
                androidVersion = x.ImkVersion.AppVersion,
                rpVersion = x.ImkVersion.RPIVersion,
                asp = x.Site.AspCompany.Name,
                date = x.VistedAt.ToString("yyyy-MM-dd"),
                //contact = x.Site.AspCompany.ApsMentor.Email
            }).ToListAsync();
            return new JsonResult(visitDetails);
        }
    }
}