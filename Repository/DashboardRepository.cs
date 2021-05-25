using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Models;
using System.Linq;
using IMK_web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using Data;
using RestSharp;
using Newtonsoft.Json;
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

        public async Task<IEnumerable<Site>> GetIMKCountriesByMA(string MA)
        {
            if (MA == null)
                return await _context.Sites.OrderBy(s => s.Country).ToListAsync();
            else
            {
                string[] countries = await _context.Countries.Where(c => c.MA.Equals(MA)).Select(x => x.Name).ToArrayAsync();
                return await _context.Sites.Where(x => countries.Contains(x.Country)).OrderBy(s => s.Country).ToListAsync();
            }
        }

        //get operators for countries
        public async Task<IEnumerable<Country>> GetOperatorsByCountry(string countries)
        {
            string[] arrCountries = countries.Split(",");
            return await _context.Countries.Where(c => arrCountries.Contains(c.Name)).Include(x => x.Operators).ToListAsync();

        }


        ////////////////////////////////////////////            ///////////////////////////////////////////
        ///////////////////////////////////////////   GRAPHS   ///////////////////////////////////////////
        //////////////////////////////////////////            ///////////////////////////////////////////

        //get unique site visits by physical location of sites (grouped by country)
        public async Task<ActionResult> GetSiteVisits(string start, string end, string countries, string operators)
        {
            List<SiteVisit> visits = null;
            if (countries == null)
            {
                visits = await _context.SiteVisits.OrderBy(s => s.StartTime.Date).Include(x => x.Site).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime.Date).Include(x => x.Site).Where(c => arrCountries.Contains(c.Site.Country)).ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime.Date).Include(x => x.Site).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country))
                        .Where(c => arrOps.Contains(c.Site.Operator.Name)).ToListAsync();
                }
            }

            var res = visits.GroupBy(x => x.StartTime.Date);
            Dictionary<string, Dictionary<string, int>> cc = new Dictionary<string, Dictionary<string, int>>();
            var date = "";
            foreach (var v in res)
            {
                date = v.First().StartTime.Date.ToString("yyyy-MM-dd");

                Dictionary<string, int> data = v.GroupBy(x => new { x.Site.Country }).Select(y => new
                {
                    country = y.Key.Country,
                    sites = y.Select(i => new
                    {
                        lat = (String.Format("{0:n4}", Convert.ToDouble(i.Site.Latitude))),
                        lon = (String.Format("{0:n4}", Convert.ToDouble(i.Site.longitude)))
                    }).Distinct().Count()
                }).ToDictionary(g => g.country, g => g.sites);

                cc.Add(date, data);
            }


            return new JsonResult(cc);
        }

        //get unique site visits by site id (grouped by country)
        public async Task<ActionResult> GetSitesByCountry(string start, string end, string countries, string operators)
        {
            List<SiteVisit> visits = null;
            if (countries == "[]" || countries == null)
            {
                visits = await _context.SiteVisits.OrderBy(s => s.StartTime).Include(x => x.Site).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime).Include(x => x.Site).Where(c => arrCountries.Contains(c.Site.Country)).ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime).Include(x => x.Site).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country))
                        .Where(c => arrOps.Contains(c.Site.Operator.Name)).ToListAsync();
                }
            }

            var res = visits.GroupBy(x => x.StartTime.Date);
            Dictionary<string, Dictionary<string, int>> cc = new Dictionary<string, Dictionary<string, int>>();
            var date = "";
            foreach (var v in res)
            {
                date = v.First().StartTime.ToString("yyyy-MM-dd");

                Dictionary<string, int> data = v.GroupBy(x => new { x.Site.Country }).Select(y => new
                {
                    country = y.Key.Country,
                    sites = y.Select(i => i.Site.SiteId).Distinct().Count()
                }).ToDictionary(g => g.country, g => g.sites);
                cc.Add(date, data);
            }

            return new JsonResult(cc);
        }


        // get site revisits during T = 12 hrs 
        public async Task<ActionResult> GetSiteRevisits(string start, string end, string countries, string operators)
        {

            List<Site> sites = null;
            if (countries == "[]" || countries == null)
            {
                sites = await _context.Sites.Include(x => x.SiteVisits).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    sites = await _context.Sites.Include(x => x.SiteVisits).Where(c => arrCountries.Contains(c.Country)).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    sites = await _context.Sites.Include(x => x.SiteVisits)
                        .Where(c => arrCountries.Contains(c.Country))
                        .Where(c => arrOps.Contains(c.Operator.Name)).ToListAsync();
                }
            }
            var res = sites.GroupBy(x => x.Country);
            Dictionary<string, Dictionary<string, int>> dict = new Dictionary<string, Dictionary<string, int>>();
            var country = "";

            foreach (var site in res)
            {
                country = site.First().Country;
                // List<Dictionary<string,int>> list = new List<Dictionary<string, int>>();
                Dictionary<string, int> siterevisit = new Dictionary<string, int>();
                int revisits = 1;


                foreach (var s in site)
                {
                    var visits = s.SiteVisits.OrderBy(x => x.StartTime).ToList();

                    if (visits.Count() > 1)
                    {
                        var T = visits.FirstOrDefault().StartTime;

                        for (int i = 1; i < visits.Count(); i++)
                        {
                            var revisitedOn = "";
                            if (visits[i].StartTime >= T.AddHours(12))
                            {
                                revisitedOn = visits[i].StartTime.Date.ToString("yyyy-MM-dd");
                                if (siterevisit.ContainsKey(revisitedOn))
                                    siterevisit[revisitedOn] = revisits++;

                                else
                                    siterevisit.Add(revisitedOn, revisits);

                            }
                            T = visits[i].StartTime;

                        }

                    }
                }
                dict.Add(country, siterevisit);
            }

            Dictionary<string, Dictionary<string, int>> bydate = new Dictionary<string, Dictionary<string, int>>();

            var dates = dict.Values.SelectMany(v => v.Keys).Distinct();

            foreach (var date in dates)
            {
                Dictionary<string, int> bycountry = new Dictionary<string, int>();

                foreach (var data in dict.ToList())
                {
                    foreach (var c in data.Value)
                    {
                        if (date.Equals(c.Key))
                        {
                            if (bycountry.ContainsKey(data.Key))
                                bycountry[data.Key] = c.Value;
                            else
                                bycountry.Add(data.Key, c.Value);
                        }
                    }
                }
                if (bydate.ContainsKey(date))
                    bydate[date] = bycountry;
                else
                    bydate.Add(date, bycountry);
            }


            return new JsonResult(bydate);

        }

        // get IMK functions used
        public async Task<ActionResult> GetIMKFunctions(string start, string end, string countries, string operators)
        {
            List<int> sfunctions = null;
            if (countries == "[]" || countries == null)
            {
                sfunctions = await _context.SiteVisits.Include("Site").Include("IMK_Functions").Select(s => s.IMK_Functions.Id).ToListAsync();
            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    sfunctions = await _context.SiteVisits.Include("Site").Include("IMK_Functions").Where(c => arrCountries.Contains(c.Site.Country)).Select(s => s.IMK_Functions.Id).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    sfunctions = await _context.SiteVisits.Include("Site").Include("IMK_Functions").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name)).Select(s => s.IMK_Functions.Id).ToListAsync();

                }
            }
            var functions = _context.IMK_Functions.Where(x => sfunctions.Contains(x.Id)).GroupBy(x => true).Select(x => new
            {

                FruStatus = x.Sum(y => y.FruStatus),
                FruState = x.Sum(y => y.FruState),
                FruSerial = x.Sum(y => y.FruSerial),
                FruProdNo = x.Sum(y => y.FruProdNo),
                RetSerial = x.Sum(y => y.RetSerial),
                TMA = x.Sum(y => y.TMA),
                RetAntenna = x.Sum(y => y.RetAntenna),
                VSWR = x.Sum(y => y.VSWR),
                CPRI = x.Sum(y => y.CPRI),
                Transport = x.Sum(y => y.Transport),
                TransportRoutes = x.Sum(y => y.TransportRoutes),
                TransportInterfaces = x.Sum(y => y.TransportInterfaces),
                MMEStatus = x.Sum(y => y.MMEStatus),
                GsmTRX = x.Sum(y => y.GsmTRX),
                GsmState = x.Sum(y => y.GsmState),
                SgwStatus = x.Sum(y => y.SgwStatus),
                Traffic3g = x.Sum(y => y.Traffic3g),
                Traffic4g = x.Sum(y => y.Traffic4g),
                Traffic5g = x.Sum(y => y.Traffic5g),
                RSSIUMTS = x.Sum(y => y.RSSIUMTS),
                RSSIFDD = x.Sum(y => y.RSSIFDD),
                RSSITDD = x.Sum(y => y.RSSITDD),
                RSSINR = x.Sum(y => y.RSSINR),
                ExternalAlarm = x.Sum(y => y.ExternalAlarm),
                Alarm = x.Sum(y => y.Alarm)
            });
            return new JsonResult(functions);
        }

        // get top 10 engineers
        public async Task<ActionResult> GetTopEngineers(string start, string end, string countries, string operators)
        {
            List<SiteVisit> allVisits = null;
            if (countries == "[]" || countries == null)
            {
                allVisits = await _context.SiteVisits.Include("Site").Include("User").ToListAsync();

            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("Site").Include("User").Where(c => arrCountries.Contains(c.Site.Country)).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include("Site").Include("User").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name)).ToListAsync();
                }

            }

            var asp = allVisits.AsEnumerable().GroupBy(x => x.User.Name).Select(y => new
            {
                name = y.Key,
                sites = y.Select(i => i.Site.SiteId).Distinct().Count()
            });
            var topasp = asp.OrderByDescending(s => s.sites).Take(10);

            return new JsonResult(topasp);
        }

        //get IMKVersion (AppVersion %)

        public async Task<ActionResult> GetAppVersion(string start, string end, string countries, string operators)
        {

            List<SiteVisit> allVisits = null;
            if (countries == "[]" || countries == null)
            {
                allVisits = await _context.SiteVisits.Include("ImkVersion").ToListAsync();

            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Where(c => arrCountries.Contains(c.Site.Country)).ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");


                    allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(c => arrOps.Contains(c.Site.Operator.Name)).ToListAsync();

                }
            }
            var versions = allVisits.GroupBy(x => new { x.ImkVersion.AppVersion }).Select(y => new
            {
                name = y.Key.AppVersion,
                values = y.Select(i => i.VisitId).Count()
            });

            return new JsonResult(versions);
        }


        //get IMKVersion (RPIVersion %)

        public async Task<ActionResult> GetRPIVersion(string start, string end, string countries, string operators)
        {
            List<SiteVisit> allVisits = null;
            if (countries == "[]" || countries == null)
            {
                allVisits = await _context.SiteVisits.Include("ImkVersion").ToListAsync();

            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Where(c => arrCountries.Contains(c.Site.Country)).ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country))
                   .Where(c => arrOps.Contains(c.Site.Operator.Name)).ToListAsync();
                }
            }

            var versions = allVisits.GroupBy(x => new { x.ImkVersion.RPIVersion }).Select(y => new
            {
                name = y.Key.RPIVersion,
                values = y.Select(i => i.VisitId).Count()
            });

            return new JsonResult(versions);
        }


        // get all site visit details
        public async Task<ActionResult> GetSiteVisitDetails(string start, string end, string countries, string operators)
        {

            List<SiteVisit> allVisits = null;

            if (countries == "[]" || countries == null)
            {
                allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Include("User").Include(x => x.User.AspCompany).ToListAsync();


            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Include(x => x.Site.Operator).Where(c => arrCountries.Contains(c.Site.Country)).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include("ImkVersion").Include("Site").Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name)).ToListAsync();
                }
            }

            var visitDetails = allVisits.OrderBy(y => y.StartTime).GroupBy(x => new { x.Site.Name, x.User.UserId, x.StartTime.Date }).Select(y => new
            {
                siteName = y.Key.Name,
                country = y.Select(i => i.Site.Country),
                user = y.Select(i => i.User.Name),
                androidVersion = y.Select(i => i.ImkVersion.AppVersion),
                rpVersion = y.Select(i => i.ImkVersion.RPIVersion),
                asp = y.Select(i => i.User.AspCompany.Name),
                date = y.Select(i => i.StartTime)
                //contact = x.Site.AspCompany.ApsMentor.Email
            });
            List<VisitDetail> uniqueVisits = new List<VisitDetail>();

            foreach (var vd in visitDetails)
            {

                if (vd.date.Count() > 1)
                {
                    var d = vd.date.ToArray();
                    var T = d[0];
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.Country = vd.country.First();
                    v1.User = vd.user.First();
                    v1.AppVersion = vd.androidVersion.First();
                    v1.RpiVersion = vd.rpVersion.First();
                    v1.ASP = vd.asp.First();
                    v1.Date = d[0].Date.ToString("yyyy-MM-dd");
                    uniqueVisits.Add(v1);

                    for (var i = 1; i < d.Length; i++)
                    {
                        if (d[i] >= T.AddHours(12))
                        {
                            VisitDetail v = new VisitDetail();
                            v.SiteName = vd.siteName;
                            v.Country = vd.country.First();
                            v.User = vd.user.First();
                            v.AppVersion = vd.androidVersion.First();
                            v.RpiVersion = vd.rpVersion.First();
                            v.ASP = vd.asp.First();
                            v.Date = d[i].Date.ToString("yyyy-MM-dd");
                            uniqueVisits.Add(v);
                        }
                        T = d[i];
                    }
                }
                else
                {
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.Country = vd.country.First();
                    v1.User = vd.user.First();
                    v1.AppVersion = vd.androidVersion.First();
                    v1.RpiVersion = vd.rpVersion.First();
                    v1.ASP = vd.asp.First();
                    v1.Date = vd.date.First().Date.ToString("yyyy-MM-dd");
                    uniqueVisits.Add(v1);
                }

            }
            return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));



        }

        ///////////////////////////////////////////////////// GLOBES /////////////////////////////////////////////////////

        // get # of sites usage (num of site visits by country)
        public async Task<ActionResult> GetSiteUsage(string start, string end, string marketArea)
        {
            var countries = await this.GetIMKCountriesByMA(marketArea);
            var c = countries.Select(c => c.Country).Distinct().ToList();

            var visits = await _context.SiteVisits.Include("Site").Where(x => c.Contains(x.Site.Country)).ToListAsync();
            var unique_visits = await _context.SiteVisits.Include("Site").Where(x => c.Contains(x.Site.Country)).Select(x => x.Site.SiteId).Distinct().ToListAsync();
            var all_usage = visits.GroupBy(x => new { x.Site.Country }).Select(y => new
            {
                country = y.Key.Country,
                usage = y.Select(i => i.Site.SiteId).Distinct().Count(),
                percent = ((float)y.Select(i => i.Site.SiteId).Distinct().Count() / (float)unique_visits.Count()) * 100

            });
            return new JsonResult(all_usage);
        }

        // get active users by country (users accompanied with site visits)
        public async Task<ActionResult> GetActiveUsers(string start, string end, string marketArea)
        {
            var countries = await this.GetIMKCountriesByMA(marketArea);
            var c = countries.Select(c => c.Country).Distinct().ToList();

            var visits = await _context.SiteVisits.Include("User").Include("Site").Where(x => c.Contains(x.Site.Country)).ToListAsync();
            var site_users = await _context.SiteVisits.Include("User").Where(x => c.Contains(x.Site.Country)).Select(x => x.User.UserId).Distinct().ToListAsync();
            var active_users = visits.GroupBy(x => new { x.Site.Country }).Select(y => new
            {
                country = y.Key.Country,
                users = y.Select(i => i.User.UserId).Distinct().Count(),
                percent = ((float)y.Select(i => i.User.UserId).Distinct().Count() / (float)site_users.Count()) * 100

            });
            return new JsonResult(active_users);
        }

        // get number of new user profiles created
        public async Task<ActionResult> GetNewProfiles(string start, string end, string marketArea)
        {
            string[] countries;
            if (marketArea.Equals("SelectAll"))
                countries = await _context.Countries.Select(x => x.Name).ToArrayAsync();
            else
                countries = await _context.Countries.Where(x => x.MA.Equals(marketArea)).Select(x => x.Name).ToArrayAsync();

            var allusers = await _context.Users.Include(x => x.AspCompany).Include(x => x.AspCompany.Country).Where(x => countries.Contains(x.AspCompany.Country.Name)).ToListAsync(); //registered at betwen start -end
            var newusers = allusers.GroupBy(x => new { x.AspCompany.Country.Name }).Select(y => new
            {
                country = y.Key.Name,
                users = y.Select(i => i.UserId).Count(),
                percent = ((float)y.Select(i => i.UserId).Count() / (float)allusers.Count()) * 100
            });
            return new JsonResult(newusers);
        }



    }
}
