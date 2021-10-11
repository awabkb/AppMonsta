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
                return new JsonResult(null);

            else if (countries == "all")
            {
                visits = await _context.SiteVisits.OrderBy(s => s.StartTime.Date).Include(x => x.Site)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime.Date).Include(x => x.Site).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date).ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime.Date).Include(x => x.Site).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country))
                        .Where(c => arrOps.Contains(c.Site.Operator.Name))
                        .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date).ToListAsync();
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
                        lat = (String.Format("{0:n4}", i.Site.Latitude)),
                        lon = (String.Format("{0:n4}", i.Site.Longitude))
                    }).Distinct().Count()
                }).OrderBy(g => g.country).ToDictionary(g => g.country, g => g.sites);

                cc.Add(date, data);
            }


            return new JsonResult(cc);
        }

        //get unique site visits by site id (grouped by country)
        public async Task<ActionResult> GetSitesByCountry(string start, string end, string countries, string operators)
        {
            List<SiteVisit> visits = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                visits = await _context.SiteVisits.OrderBy(s => s.StartTime).Include(x => x.Site)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime).Include(x => x.Site).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date).ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    visits = await _context.SiteVisits.OrderBy(s => s.StartTime).Include(x => x.Site).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country))
                        .Where(c => arrOps.Contains(c.Site.Operator.Name))
                        .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date).ToListAsync();
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
                }).OrderBy(g => g.country).ToDictionary(g => g.country, g => g.sites);
                cc.Add(date, data);
            }

            return new JsonResult(cc);
        }


        // get site revisits during T = 12 hrs 
        public async Task<ActionResult> GetSiteRevisits(string start, string end, string countries, string operators)
        {

            List<Site> sites = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                sites = await _context.Sites.Include(x => x.SiteVisits.Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    sites = await _context.Sites.Include(x => x.SiteVisits.Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date))
                    .Where(c => arrCountries.Contains(c.Country)).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    sites = await _context.Sites.Include(x => x.SiteVisits.Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date))
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
                                    siterevisit[revisitedOn]++;

                                else
                                    siterevisit.Add(revisitedOn, 1);

                            }
                            T = visits[i].StartTime;

                        }

                    }
                }
                dict.Add(country, siterevisit);
            }
            dict.OrderBy(x => x.Key);
            Dictionary<string, Dictionary<string, int>> bydate = new Dictionary<string, Dictionary<string, int>>();

            var dates = dict.Values.SelectMany(v => v.Keys).Distinct().OrderBy(x => x);
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
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                sfunctions = await _context.SiteVisits.Include("Site").Include("IMK_Functions")
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .Select(s => s.IMK_Functions.Id).ToListAsync();
            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    sfunctions = await _context.SiteVisits.Include("Site").Include("IMK_Functions").Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .Select(s => s.IMK_Functions.Id).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    sfunctions = await _context.SiteVisits.Include("Site").Include("IMK_Functions").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .Select(s => s.IMK_Functions.Id).ToListAsync();

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
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits.Include("Site").Include("User")
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();

            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("Site").Include("User").Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include("Site").Include("User").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }

            }

            var asp = allVisits.AsEnumerable().GroupBy(x => x.User.Name).Select(y => new
            {
                name = y.Key + " - " + (y.Select(i=>i.Site.Country).First()),
                sites = y.Select(i => i.Site.SiteId).Distinct().Count()
            });
            var topasp = asp.OrderByDescending(s => s.sites).Take(10);

            return new JsonResult(topasp);
        }

        //get IMKVersion (AppVersion %)

        public async Task<ActionResult> GetAppVersion(string start, string end, string countries, string operators)
        {

            List<SiteVisit> allVisits = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();

            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("Site").Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");


                    allVisits = await _context.SiteVisits.Include("Site").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(c => arrOps.Contains(c.Site.Operator.Name))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();

                }
            }
            var versions = allVisits.GroupBy(x => x.AppVersion.ToString("0.00")).Select(y => new
            {
                name = y.Key,
                values = y.Select(i => i.VisitId).Count()
            });

            return new JsonResult(versions);
        }


        //get IMKVersion (RPIVersion %)

        public async Task<ActionResult> GetRPIVersion(string start, string end, string countries, string operators)
        {
            List<SiteVisit> allVisits = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();

            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("Site").Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {

                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    allVisits = await _context.SiteVisits.Include("Site").Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country))
                   .Where(c => arrOps.Contains(c.Site.Operator.Name))
                   .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                   .ToListAsync();
                }
            }

            var versions = allVisits.GroupBy(x => new { x.RPIVersion }).Select(y => new
            {
                name = ((double)y.Key.RPIVersion).ToString("0.00"),
                values = y.Select(i => i.VisitId).Count()
            });

            return new JsonResult(versions);
        }



        // get all site visit details
        public async Task<ActionResult> GetSiteVisitDetails(string start, string end, string countries, string operators)
        {

            List<SiteVisit> allVisits = null;

            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits.Include("Site").Include("User").Include(x => x.User.AspCompany)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include("Site").Include("User").Include(x => x.User.AspCompany).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include("Site").Include("User").Include(x => x.User.AspCompany).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                        .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                        .ToListAsync();
                }
            }

            var visitDetails = allVisits.OrderBy(y => y.StartTime).Where(y => y.Site.Name != null).GroupBy(x => new { x.Site.Name, x.User.UserId, x.StartTime.Date }).Select(y => new
            {
                siteName = y.Key.Name,
                country = y.Select(i => i.Site.Country),
                user = y.Select(i => i.User.Name),
                phone = y.Select(i => i.User.Phone),
                email = y.Select(i => i.User.Email),
                androidVersion = y.Select(i => i.AppVersion),
                rpVersion = y.Select(i => i.RPIVersion),
                asp = y.Select(i => i.User.AspCompany.Name),
                date = y.Select(i => i.StartTime)
                //contact = x.Site.AspCompany.ApsMentor.Email
            });
            List<VisitDetail> uniqueVisits = new List<VisitDetail>();
            Dictionary<string, DateTime> siterevisit = new Dictionary<string, DateTime>();

            foreach (var vd in visitDetails)
            {
                var revisit = false;
                if (siterevisit.ContainsKey(vd.siteName))
                {
                    var T = siterevisit[vd.siteName];
                    if (vd.date.First() >= T.AddHours(12))
                    {
                        revisit = true;
                        siterevisit[vd.siteName] = vd.date.First();
                    }
                }
                else
                    siterevisit.Add(vd.siteName, vd.date.First());

                if (vd.date.Count() > 1)
                {
                    var d = vd.date.ToArray();
                    var T = d[0];
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.Country = vd.country.First();
                    v1.User = vd.user.First();
                    v1.Phone = vd.phone.First();
                    v1.Email = vd.email.First();
                    v1.AppVersion = ((double)vd.androidVersion.First()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.First()).ToString("0.00");
                    v1.ASP = vd.asp.First();
                    v1.Date = d[0].Date.ToString("yyyy-MM-dd");
                    v1.IsRevisit = revisit;
                    uniqueVisits.Add(v1);
                    


                    for (var i = 1; i < d.Length; i++)
                    {
                        if (d[i] >= T.AddHours(12))
                        {
                            VisitDetail v = new VisitDetail();
                            v.SiteName = vd.siteName;
                            v.Country = vd.country.First();
                            v.User = vd.user.First();
                            v.Phone = vd.phone.First();
                            v.Email = vd.email.First();
                            v.AppVersion = ((double)vd.androidVersion.First()).ToString("0.00");
                            v.RpiVersion = ((double)vd.rpVersion.First()).ToString("0.00");
                            v.ASP = vd.asp.First();
                            v.Date = d[i].Date.ToString("yyyy-MM-dd");
                            v.IsRevisit = revisit;
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
                    v1.Phone = vd.phone.First();
                    v1.Email = vd.email.First();
                    v1.AppVersion = ((double)vd.androidVersion.First()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.First()).ToString("0.00");
                    v1.ASP = vd.asp.First();
                    v1.Date = vd.date.First().Date.ToString("yyyy-MM-dd");
                    v1.IsRevisit = revisit;
                    uniqueVisits.Add(v1);
                }

            }
            // return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));
            return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));

        }




        ///////////////////////////////////////////////////// GLOBES /////////////////////////////////////////////////////

        // get # of sites usage (num of site visits by country)
        public async Task<ActionResult> GetSiteUsage(string start, string end, string marketArea)
        {
            var countries = await this.GetIMKCountriesByMA(marketArea);
            var c = countries.Select(c => c.Country).Distinct().ToList();

            var visits = await _context.SiteVisits.Include("Site").Where(x => c.Contains(x.Site.Country))
            .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
            .ToListAsync();

            var unique_visits = await _context.SiteVisits.Include("Site").Where(x => c.Contains(x.Site.Country))
            .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
            .Select(x => x.Site.SiteId).Distinct().ToListAsync();

            var all_usage = visits.GroupBy(x => new { x.Site.Country }).Select(y => new
            {
                country = y.Key.Country,
                usage = y.Select(i => i.Site.SiteId).Distinct().Count(),
                percent = Math.Round(((float)y.Select(i => i.Site.SiteId).Distinct().Count() / (float)unique_visits.Count()) * 100, 1)

            });
            return new JsonResult(all_usage);
        }

        // get active users by country (users accompanied with site visits)
        public async Task<ActionResult> GetActiveUsers(string start, string end, string marketArea)
        {
            var countries = await this.GetIMKCountriesByMA(marketArea);
            var c = countries.Select(c => c.Country).Distinct().ToList();

            var visits = await _context.SiteVisits.Include("User").Include("Site").Where(x => c.Contains(x.Site.Country))
            .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
            .ToListAsync();

            var site_users = await _context.SiteVisits.Include("User").Where(x => c.Contains(x.Site.Country))
            .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
            .Select(x => x.User.UserId).Distinct().ToListAsync();
            
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
            if (marketArea == null)
                countries = await _context.Countries.Select(x => x.Name).ToArrayAsync();
            else
                countries = await _context.Countries.Where(x => x.MA.Equals(marketArea)).Select(x => x.Name).ToArrayAsync();

            var allusers = await _context.Users.Include(x => x.AspCompany).Include(x => x.AspCompany.Country)
            .Where(x => x.IsActive == true)
            .Where(x => countries.Contains(x.AspCompany.Country.Name))
            .Where(x => x.RegisteredAt.Date >= Convert.ToDateTime(start).Date && x.RegisteredAt.Date <= Convert.ToDateTime(end).Date)
            .ToListAsync();

            var newusers = allusers.GroupBy(x => new { x.AspCompany.Country.Name }).Select(y => new
            {
                country = y.Key.Name,
                users = y.Select(i => i.UserId).Count(),
                percent = ((float)y.Select(i => i.UserId).Count() / (float)allusers.Count()) * 100
            });
            return new JsonResult(newusers);
        }

        public string GetRole(string email)
        {
            var role = _context.AspManagers.Where(x => x.Email.Equals(email)).Select(x => x.Role).FirstOrDefault();
            return role;
        }






        ///////////////////////////////////////////////////// ANALYTICS /////////////////////////////////////////////////////

        public async Task<ActionResult> GetTopRevisits(string start, string end, string countries, string operators)
        {

            List<Site> sites = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                sites = await _context.Sites.Include(x => x.SiteVisits.Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date))
                .Include(x => x.SiteVisits).ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    sites = await _context.Sites.Include(x => x.SiteVisits.Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date))
                    .Where(c => arrCountries.Contains(c.Country)).ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");
                    sites = await _context.Sites.Include(x => x.SiteVisits.Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date))
                        .Where(c => arrCountries.Contains(c.Country))
                        .Where(c => arrOps.Contains(c.Operator.Name)).ToListAsync();
                }
            }
            var res = sites.GroupBy(x => x.Name);
            Dictionary<string, Dictionary<string, int>> dict = new Dictionary<string, Dictionary<string, int>>();
            var siteName = "";

            foreach (var site in res)
            {
                siteName = site.First().Name;
                // List<Dictionary<string,int>> list = new List<Dictionary<string, int>>();
                Dictionary<string, int> siterevisit = new Dictionary<string, int>();

                foreach (var s in site)
                {
                    var visits = s.SiteVisits.OrderBy(x => x.StartTime).ToList();

                    if (visits.Count() > 1)
                    {
                        var T = visits.FirstOrDefault().StartTime;

                        for (int i = 1; i < visits.Count(); i++)
                        {
                            var country = "";
                            if (visits[i].StartTime >= T.AddHours(12))
                            {
                                country = visits[i].Site.Country;
                                if (siterevisit.ContainsKey(country))
                                    siterevisit[country]++;

                                else
                                    siterevisit.Add(country, 1);

                            }
                            T = visits[i].StartTime;

                        }

                    }
                }
                if(siterevisit.Count != 0)
                  dict.Add(siteName, siterevisit);
            }
            
            // order by number of revisits and return first 10 elements
            dict = dict.OrderByDescending(x=> x.Value.Values.Sum())
            .ToDictionary(x => x.Key, x=>x.Value.OrderByDescending(y=>y.Value)
            .ToDictionary(y=>y.Key, y=>y.Value))
            .Take(10).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new JsonResult(dict);

        }
        
        // IMK functions Pass/Fail status 
        public async Task<ActionResult> GetCommandStatus(string start, string end, string countries, string operators)
        {
            List<Dictionary<string,int>> returnList = new List<Dictionary<string, int>>();

            List<SiteVisit> visitLogs = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                visitLogs = await _context.SiteVisits.Include(x => x.Logs)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();
                
            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    visitLogs = await _context.SiteVisits.Include("Site").Include(x => x.Logs).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    visitLogs = await _context.SiteVisits.Include("Site").Include(x => x.Logs).Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();

                }
            }

            
            Dictionary<string, int> pCommands  = new Dictionary<string, int>(); //passed
            Dictionary<string, int> fCommands  = new Dictionary<string, int>(); //failed


            var check1 = new List<Object>();

            foreach(var site in visitLogs)
            {
                foreach(var logs in site.Logs)
                {
                    if(logs.Result != null) {
                        var command = logs.Command;
                        dynamic results = JsonConvert.DeserializeObject(logs.Result);
                        var passed = 0;
                        if(results != null)
                        {
                            switch(command) {
                                case "vswr": 
                                    foreach(var result in results)
                                    {   
                                        String status = result.STATUS;
                                        if(status.Equals("PASSED"))
                                            passed = 1;
                                        else if(status.Equals("FAILED")) {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if(passed == 1)
                                        if(pCommands.ContainsKey("vswr"))
                                            pCommands["vswr"] ++;
                                        else
                                            pCommands.Add("vswr", 1);

                                    else
                                        if(fCommands.ContainsKey("vswr"))
                                            fCommands["vswr"] ++;
                                        else
                                            fCommands.Add("vswr", 1);                                

                                break;

                                    case "rssi_umts":
                                    foreach(var result in results)
                                    {   
                                        String status = result.CELL;
                                        if(status.Equals("PASSED"))
                                            passed = 1;
                                        else if(status.Equals("FAILED")) {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if(passed == 1)
                                        if(pCommands.ContainsKey("umts"))
                                            pCommands["umts"] ++;
                                        else
                                            pCommands.Add("vswr", 1);

                                    else
                                        if(fCommands.ContainsKey("umts"))
                                            fCommands["umts"] ++;
                                        else
                                            fCommands.Add("umts", 1);                                

                                break;

                                    case "rssi-lte EUtranCellFDD":
                                    foreach(var result in results)
                                    {   double rssi;
                                        bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                        if(isValue == true && rssi <= -110)
                                            passed = 1;
                                        else if(isValue == false || rssi > -110) {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if(passed == 1)
                                        if(pCommands.ContainsKey("fdd"))
                                            pCommands["fdd"] ++;
                                        else
                                            pCommands.Add("fdd", 1);

                                    else
                                        if(fCommands.ContainsKey("fdd"))
                                            fCommands["fdd"] ++;
                                        else
                                            fCommands.Add("fdd", 1);                                

                                break;

                                    case "rssi-lte EUtranCellTDD":
                                    foreach(var result in results)
                                    {   
                                        double rssi;
                                        bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                        if(isValue == true && rssi <= -110)
                                            passed = 1;
                                        else if(isValue == false || rssi > -110) {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if(passed == 1)
                                        if(pCommands.ContainsKey("tdd"))
                                            pCommands["tdd"] ++;
                                        else
                                            pCommands.Add("tdd", 1);

                                    else
                                        if(fCommands.ContainsKey("tdd"))
                                            fCommands["tdd"] ++;
                                        else
                                            fCommands.Add("tdd", 1);                                

                                break;

                                    case "rssi-nr":
                                    foreach(var result in results)
                                    {   
                                        double rssi;
                                        bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                        if(isValue == true && rssi <= -110)
                                            passed = 1;
                                        else if(isValue == false || rssi > -110) {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if(passed == 1)
                                        if(pCommands.ContainsKey("nr"))
                                            pCommands["nr"] ++;
                                        else
                                            pCommands.Add("nr", 1);

                                    else
                                        if(fCommands.ContainsKey("nr"))
                                            fCommands["nr"] ++;
                                        else
                                            fCommands.Add("nr", 1);                                

                                break;

                                    case "alarm":
                                    foreach(var result in results)
                                    {   
                                        String description = result.DESCRIPTION;
                                        if(description.Equals(""))
                                            passed = 1;
                                        else if(!description.Equals("")) {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if(passed == 1)
                                        if(pCommands.ContainsKey("alarm"))
                                            pCommands["alarm"] ++;
                                        else
                                            pCommands.Add("alarm", 1);

                                    else
                                        if(fCommands.ContainsKey("alarm"))
                                            fCommands["alarm"] ++;
                                        else
                                            fCommands.Add("alarm", 1);                                

                                break;
                            }
                        }

                    }
                }
            }
            returnList.Add(pCommands);
            returnList.Add(fCommands);
            return new JsonResult(returnList);
        
        }


        // Number of resolved failures 
        public async Task<ActionResult> GetResolvedFailures(string start, string end, string countries, string operators)
        {
            List<SiteVisit> visitLogs = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                visitLogs = await _context.SiteVisits.Include(x => x.Logs).Include("Site")
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
                
            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    visitLogs = await _context.SiteVisits.Include("Site").Include(x => x.Logs).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .OrderBy(x => x.StartTime)
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    visitLogs = await _context.SiteVisits.Include("Site").Include(x => x.Logs).Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .OrderBy(x => x.StartTime)
                    .ToListAsync();

                }
            }
            var visitsPerSite = visitLogs.GroupBy(x => x.Site.SiteId);
            Dictionary<string, int> resolvedPerCountry = new Dictionary<string, int>();

            var check1 = new List<Object>();

            // calculate resolved per each site (resolved: if passed command follows a failed command on same site)
            foreach(var site in visitsPerSite)
            {
                var country = site.First().Site.Country;
                foreach(var logs in site)
                {
                    foreach(var log in logs.Logs) {
                        var command = log.Command;
                        check1.Add(command);

                        var result = JsonConvert.DeserializeObject(log.Result);
                    }

                }
            }


            var check = new List<Object>();

            foreach(var log in visitLogs)
            {
                foreach(var result in log.Logs)
                {
                    var value = JsonConvert.DeserializeObject(result.Result);
                    check.Add(value);

                }
            }
            return new JsonResult(visitsPerSite);

        }
    
        // Get LMT Site Integrations
        public async Task<ActionResult> GetSiteIntegrations(string start, string end, string countries, string operators)
        {
            List<SiteIntegration> siteIntegrations = null;

            if (countries == null)
                return new JsonResult(null);

            else
            {
                siteIntegrations = await _context.SiteIntegrations.Where(x => x.SiteName != null).ToListAsync();
            }

            return new JsonResult(siteIntegrations);

        }
    }

}
