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
using MathNet.Numerics.Statistics;
using IMK_web.Services;

namespace IMK_web.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DataContext _context;
        private readonly IIMKHelperService _IMKHelperservice;

        public DashboardRepository(DataContext dataContext, IIMKHelperService imkHelperService)
        {
            this._context = dataContext;
            _IMKHelperservice = imkHelperService;
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
                date = v.FirstOrDefault().StartTime.Date.ToString("yyyy-MM-dd");

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
                date = v.FirstOrDefault().StartTime.ToString("yyyy-MM-dd");

                Dictionary<string, int> data = v.GroupBy(x => new { x.Site.Country }).Select(y => new
                {
                    country = y.Key.Country,
                    sites = y.Select(i => i.Site.SiteId).Distinct().Count()
                }).OrderBy(g => g.country).ToDictionary(g => g.country, g => g.sites);
                cc.Add(date, data);
            }

            return new JsonResult(cc);
        }


        // get site revisits during T = 8 hrs 
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
                country = site.FirstOrDefault().Country;
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
                            if (visits[i].StartTime >= T.AddHours(8))
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
                name = y.Key + " - " + (y.Select(i => i.Site.Country).FirstOrDefault()),
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

            return new JsonResult(versions.OrderByDescending(x => x.values));
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

            return new JsonResult(versions.OrderByDescending(x => x.values));
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
                operatorName = y.Select(i => i.Site.Operator.Name),
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
                    if (vd.date.FirstOrDefault() >= T.AddHours(8))
                    {
                        revisit = true;
                        siterevisit[vd.siteName] = vd.date.FirstOrDefault();
                    }
                }
                else
                    siterevisit.Add(vd.siteName, vd.date.FirstOrDefault());

                if (vd.date.Count() > 1)
                {
                    var d = vd.date.ToArray();
                    var T = d[0];
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.OperatorName = vd.operatorName.FirstOrDefault();
                    v1.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Phone = vd.phone.FirstOrDefault();
                    v1.Email = vd.email.FirstOrDefault();
                    v1.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                    v1.ASP = vd.asp.FirstOrDefault();
                    v1.Date = d[0].Date.ToString("yyyy-MM-dd");
                    v1.IsRevisit = revisit;
                    uniqueVisits.Add(v1);



                    for (var i = 1; i < d.Length; i++)
                    {
                        if (d[i] >= T.AddHours(8))
                        {
                            VisitDetail v = new VisitDetail();
                            v.SiteName = vd.siteName;
                            v.OperatorName = vd.operatorName.FirstOrDefault();
                            v.Country = vd.country.FirstOrDefault();
                            v.User = vd.user.FirstOrDefault();
                            v.Phone = vd.phone.FirstOrDefault();
                            v.Email = vd.email.FirstOrDefault();
                            v.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                            v.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                            v.ASP = vd.asp.FirstOrDefault();
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
                    v1.OperatorName = vd.operatorName.FirstOrDefault();
                    v1.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Phone = vd.phone.FirstOrDefault();
                    v1.Email = vd.email.FirstOrDefault();
                    v1.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                    v1.ASP = vd.asp.FirstOrDefault();
                    v1.Date = vd.date.FirstOrDefault().Date.ToString("yyyy-MM-dd");
                    v1.IsRevisit = revisit;
                    uniqueVisits.Add(v1);
                }

            }
            // return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));
            return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));

        }

        public async Task<ActionResult> GetSiteVisitDetailsNew(string start, string end, string countries, string operators)
        {
            List<SiteVisit> allVisits = null;
            List<IntegrationDetail> lmts = new List<IntegrationDetail>();

            var siteIntegrations = new List<IntegrationDetail>();
            //get diagnostics
            var _siteIntegrations = _context.SiteIntegrations.Where(x => x.SiteName != null)
                         .Where(x => x.DownloadStart != null)
                         .OrderByDescending(x => x.DownloadStart).ToList();

            var integrations = _siteIntegrations
            .Where(x => Convert.ToDateTime(x.DownloadStart).Date >= Convert.ToDateTime(start).Date && Convert.ToDateTime(x.DownloadStart).Date <= Convert.ToDateTime(end).Date)
            .GroupBy(x => new
            {
                x.SiteName,
                x.UserId,
                start = x.DownloadStart != null ? Convert.ToDateTime(x.DownloadStart).Date : Convert.ToDateTime(x.IntegrateEnd).Date

            });
            List<IntegrationDetail> filteredIntegrations = null;
            foreach (var integration in integrations)
            {

                User user = await this.GetUser(integration.FirstOrDefault().UserId);
                Site site = await this.GetSite(integration.FirstOrDefault().SiteName, integration.FirstOrDefault().CountryName);

                IntegrationDetail visit = new IntegrationDetail();
                visit.SiteName = integration.FirstOrDefault().SiteName;
                visit.Country = integration.FirstOrDefault().CountryName == null ? user.AspCompany.Country.Name : integration.FirstOrDefault().CountryName;
                visit.User = user.Name;
                visit.Asp = user.AspCompany.Name;
                visit.Operator = site == null ? null : site.Operator.Name;
                visit.DownloadStart = integration.FirstOrDefault().DownloadStart.ToString();
                visit.DownloadEnd = integration.Last().DownloadEnd?.ToString();
                visit.IntegrateStart = integration.FirstOrDefault().IntegrateStart;
                visit.IntegrateEnd = integration.Last().IntegrateEnd;
                visit.Outcome = integration.Last().Outcome;
                visit.IntegrationTime = String.IsNullOrEmpty(visit.DownloadStart) || String.IsNullOrEmpty(visit.IntegrateEnd) ?
                    "0 mins" : ((int)(Convert.ToDateTime(visit.IntegrateEnd) - Convert.ToDateTime(visit.DownloadStart)).TotalMinutes).ToString() + " mins";
                visit.AndroidVersion = integration.FirstOrDefault().AppVersion;
                lmts.Add(visit);

            }


            if (countries == null)
                return new JsonResult(null);
            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm") && !String.IsNullOrEmpty(y.Result))).Include("Site").Include("User").Include(x => x.User.AspCompany)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();
                filteredIntegrations = lmts;

            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm") && !String.IsNullOrEmpty(y.Result))).Include("Site").Include("User").Include(x => x.User.AspCompany).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                    filteredIntegrations = lmts.Where(c => arrCountries.Contains(c.Country)).ToList();

                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm") && !String.IsNullOrEmpty(y.Result))).Include("Site").Include("User").Include(x => x.User.AspCompany).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                        .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                        .ToListAsync();
                    filteredIntegrations = lmts.Where(c => arrCountries.Contains(c.Country)).Where(c => arrOps.Contains(c.Operator)).ToList();

                }
            }

            var visitDetails = allVisits.OrderBy(y => y.StartTime).Where(y => y.Site.Name != null).GroupBy(x => new { x.Site.Name, x.User.UserId, x.StartTime.Date }).Select(y => new
            {
                siteName = y.Key.Name,
                operatorName = y.Select(i => i.Site.Operator?.Name),
                country = y.Select(i => i.Site.Country),
                user = y.Select(i => i.User.Name),
                phone = y.Select(i => i.User.Phone),
                email = y.Select(i => i.User.Email),
                androidVersion = y.Select(i => i.AppVersion),
                rpVersion = y.Select(i => i.RPIVersion),
                asp = y.Select(i => i.User.AspCompany.Name),
                date = y.Select(i => i.StartTime),
                id = y.Select(i => i.VisitId),
                logs = y.Select(i => i.Logs)
                //contact = x.Site.AspCompany.ApsMentor.Email
            });
            List<VisitDetail> uniqueVisits = new List<VisitDetail>();
            Dictionary<string, DateTime> siterevisit = new Dictionary<string, DateTime>();

            foreach (var vd in visitDetails)
            {
                var revisit = false;
                var lmtRecord = filteredIntegrations.FirstOrDefault(item => item.SiteName == vd.siteName);

                if (lmtRecord != null && vd.date.FirstOrDefault() <= Convert.ToDateTime(lmtRecord.DownloadStart).AddHours(8))
                {
                    filteredIntegrations.Remove(lmtRecord);

                }
                else if (lmtRecord != null && vd.date.FirstOrDefault() >= Convert.ToDateTime(lmtRecord.DownloadStart).AddHours(8))
                {
                    if (siterevisit.ContainsKey(vd.siteName))
                    {
                        var T = siterevisit[vd.siteName];
                        if (vd.date.FirstOrDefault() >= T.AddHours(8))
                        {
                            revisit = true;
                            siterevisit[vd.siteName] = Convert.ToDateTime(lmtRecord.DownloadStart);
                        }
                    }
                    else
                        siterevisit.Add(vd.siteName, Convert.ToDateTime(lmtRecord.DownloadStart));
                    lmtRecord = null;

                }
                else
                {
                    lmtRecord = null;
                }
                if (siterevisit.ContainsKey(vd.siteName))
                {
                    var T = siterevisit[vd.siteName];
                    if (vd.date.FirstOrDefault() >= T.AddHours(8))
                    {
                        revisit = true;
                        siterevisit[vd.siteName] = vd.date.FirstOrDefault();
                    }
                }
                else
                    siterevisit.Add(vd.siteName, vd.date.FirstOrDefault());

                if (vd.date.Count() > 1)
                {
                    var _a = new List<Log>(); ;

                    vd.logs.ToList().ForEach(l =>
                    {
                        l.ToList().ForEach(sl =>
                        {
                            _a.Add(sl);
                        });
                    });

                    var d = vd.date.ToArray();
                    var T = d[0];
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.OperatorName = vd.operatorName.FirstOrDefault();
                    v1.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Phone = vd.phone.FirstOrDefault();
                    v1.Email = vd.email.FirstOrDefault();
                    v1.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                    v1.ASP = vd.asp.FirstOrDefault();
                    v1.Date = d[0].ToString("yyyy-MM-dd HH:mm");
                    v1.IsRevisit = revisit;
                    v1.Logs = _a;
                    //v1.SiteIntegration = integrations.FirstOrDefault(item => item.Key.SiteName == y.Key.Name);
                    v1.SiteIntegration = lmtRecord;
                    v1.Diagnostic = true;
                    //v1.IsIntegrated = vd.isIntegrated;
                    uniqueVisits.Add(v1);

                    for (var i = 1; i < d.Length; i++)
                    {

                        if (d[i] >= T.AddHours(8))
                        {
                            var a = new List<Log>(); ;

                            vd.logs.ToList().ForEach(l =>
                            {
                                l.ToList().ForEach(sl =>
                                {
                                    a.Add(sl);
                                });
                            });
                            VisitDetail v = new VisitDetail();
                            v.SiteName = vd.siteName;
                            v.OperatorName = vd.operatorName.FirstOrDefault();
                            v.Country = vd.country.FirstOrDefault();
                            v.User = vd.user.FirstOrDefault();
                            v.Phone = vd.phone.FirstOrDefault();
                            v.Email = vd.email.FirstOrDefault();
                            v.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                            v.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                            v.ASP = vd.asp.FirstOrDefault();
                            v.Date = d[i].ToString("yyyy-MM-dd HH:mm");
                            v.IsRevisit = revisit;
                            v.Logs = a;
                            //  v.Diagnostic = logs.FirstOrDefault(item => item.SiteVisit.VisitId == vd.id.FirstOrDefault());
                            v.SiteIntegration = lmtRecord;
                            v.Diagnostic = true;
                            uniqueVisits.Add(v);
                        }

                        T = d[i];
                    }
                }
                else
                {
                    var a = new List<Log>(); ;

                    vd.logs.ToList().ForEach(l =>
                    {
                        l.ToList().ForEach(sl =>
                        {
                            a.Add(sl);
                        });
                    });
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.OperatorName = vd.operatorName.FirstOrDefault();
                    v1.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Phone = vd.phone.FirstOrDefault();
                    v1.Email = vd.email.FirstOrDefault();
                    v1.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                    v1.ASP = vd.asp.FirstOrDefault();
                    v1.Date = vd.date.FirstOrDefault().ToString("yyyy-MM-dd HH:mm");
                    v1.IsRevisit = revisit;
                    v1.SiteIntegration = lmtRecord;
                    v1.Diagnostic = true;
                    v1.Logs = a;
                    uniqueVisits.Add(v1);
                }

            }

            foreach (var visit in uniqueVisits.OrderBy(x => x.Date))
            {
                Dictionary<string, int> alarmTypes = new Dictionary<string, int>();
                Dictionary<string, string> commands = new Dictionary<string, string>();
                List<string> alarms = new List<string>();
                IEnumerable<string> clearedAlarms = new List<string>();
                visit.FTR = null;
                visit.AlarmClearTime = null;
                visit.AlarmTime = null;
                //////////// Alarms
                bool recorded = false;
                bool alarmsChecked = false;

                foreach (var log in visit.Logs.OrderBy(x => x.TimeOfAction))
                {
                    List<string> currentAlarms = new List<string>();

                    if (log.Command.Equals("alarm"))
                    {
                        dynamic results = null;


                        alarmsChecked = true;

                        results = JsonConvert.DeserializeObject(log.Result);
                        if (results != null)
                        {
                            foreach (var alarm in results)
                            {
                                string alarmType = (alarm.DESCRIPTION).ToString().Split(".")[0];
                                if (GetAlarmType(alarmType).Equals("Field"))
                                {
                                    if (!commands.ContainsKey(log.Command)) //first alarm attempt
                                    {
                                        if (!alarms.Contains(alarmType)) alarms.Add(alarmType);
                                    }
                                    if (alarmTypes.ContainsKey(alarmType)) alarmTypes[alarmType]++;
                                    else alarmTypes[alarmType] = 1;
                                    if (!currentAlarms.Contains(alarmType)) currentAlarms.Add(alarmType);
                                    if (!recorded)
                                    {
                                        visit.Date = log.TimeOfAction.ToString("yyyy-MM-dd HH:mm");
                                        recorded = true;
                                    }
                                }

                            }
                        }
                        clearedAlarms = alarms.Except(currentAlarms);
                        IEnumerable<string> newAlarms = currentAlarms.Except(alarms);

                        foreach (var item in newAlarms) alarms.Add(item);
                    }





                }
                if (!clearedAlarms.Any() && !alarms.Any() && alarmsChecked)
                {
                    visit.FTR = true;
                    visit.AlarmClearTime = null;
                }
                else if (clearedAlarms.Count() == alarms.Count() && alarmsChecked)
                {
                    if (visit.SiteIntegration != null)
                    {
                        if (visit.Logs.Last().TimeOfAction <= Convert.ToDateTime(visit.SiteIntegration.DownloadStart).AddHours(8))
                        {
                            visit.FTR = true;
                            alarms.RemoveAll(a => clearedAlarms.Contains(a));
                            visit.AlarmClearTime = visit.Logs.Last().TimeOfAction.ToString("yyyy-MM-dd HH:mm");
                            //log.TimeOfAction.ToString("yyyy-MM-dd HH:mm");
                        }

                    }
                    else
                    {
                        visit.FTR = true;
                        alarms.RemoveAll(a => clearedAlarms.Contains(a));
                        visit.AlarmClearTime = visit.Logs.Last().TimeOfAction.ToString("yyyy-MM-dd HH:mm");
                    }
                }


                else if (alarms.Count() > clearedAlarms.Count() || !alarmsChecked)
                {
                    visit.FTR = false;
                    visit.AlarmClearTime = null;
                }
                else
                {
                    continue;
                }
                visit.Logs = null;

            }

            uniqueVisits.AddRange(filteredIntegrations.OrderByDescending(x => x.DownloadStart).Select(e => new VisitDetail
            {
                SiteName = e.SiteName,
                OperatorName = e.Operator,
                Country = e.Country,
                SiteIntegration = e,
                Diagnostic = false,
                IsRevisit = false,
                Date = e.DownloadStart,
                FTR = null

            })); ;
            // return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));
            return new JsonResult(uniqueVisits.OrderByDescending(x => x.Date));

        }

        /* public async Task<ActionResult> GetData (string start, string end, string countries, string operators)
         {
             List<SiteVisit> allVisits = null;
             List<IntegrationDetail> lmts = new List<IntegrationDetail>();

             var siteIntegrations = new List<IntegrationDetail>();
             //get diagnostics
             var _siteIntegrations = _context.SiteIntegrations.Where(x => x.SiteName != null)
                          .Where(x => x.DownloadStart != null)
                          .OrderByDescending(x => x.DownloadStart).ToList();

             var integrations = _siteIntegrations
             .Where(x => Convert.ToDateTime(x.DownloadStart).Date >= Convert.ToDateTime(start).Date && Convert.ToDateTime(x.DownloadStart).Date <= Convert.ToDateTime(end).Date)
             .GroupBy(x => new
             {
                 x.SiteName,
                 x.UserId,
                 start = x.DownloadStart != null ? Convert.ToDateTime(x.DownloadStart).Date : Convert.ToDateTime(x.IntegrateEnd).Date

             });
             List<IntegrationDetail> filteredIntegrations = null;
             foreach (var integration in integrations)
             {

                 User user = await this.GetUser(integration.FirstOrDefault().UserId);
                 Site site = await this.GetSite(integration.FirstOrDefault().SiteName, integration.FirstOrDefault().CountryName);

                 IntegrationDetail visit = new IntegrationDetail();
                 visit.SiteName = integration.FirstOrDefault().SiteName;
                 visit.Country = integration.FirstOrDefault().CountryName == null ? user.AspCompany.Country.Name : integration.FirstOrDefault().CountryName;
                 visit.User = user.Name;
                 visit.Asp = user.AspCompany.Name;
                 visit.Operator = site == null ? null : site.Operator.Name;
                 visit.DownloadStart = integration.FirstOrDefault().DownloadStart.ToString();
                 visit.DownloadEnd = integration.Last().DownloadEnd?.ToString();
                 visit.IntegrateStart = integration.FirstOrDefault().IntegrateStart;
                 visit.IntegrateEnd = integration.Last().IntegrateEnd;
                 visit.Outcome = integration.Last().Outcome;
                 visit.IntegrationTime = String.IsNullOrEmpty(visit.DownloadStart) || String.IsNullOrEmpty(visit.IntegrateEnd) ?
                     "0 mins" : ((int)(Convert.ToDateTime(visit.IntegrateEnd) - Convert.ToDateTime(visit.DownloadStart)).TotalMinutes).ToString() + " mins";
                 visit.AndroidVersion = integration.FirstOrDefault().AppVersion;
                 lmts.Add(visit);

             }
             if (countries == null)
                 return new JsonResult(null);
             else if (countries == "all")
             {
                 allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm") && !String.IsNullOrEmpty(y.Result))).Include("Site").Include("User").Include(x => x.User.AspCompany)
                 .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                 .ToListAsync();
                 filteredIntegrations = lmts;

             }
             else
             {
                 if (operators == null)
                 {
                     string[] arrCountries = countries.Split(",");
                     allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm") && !String.IsNullOrEmpty(y.Result))).Include("Site").Include("User").Include(x => x.User.AspCompany).Where(c => arrCountries.Contains(c.Site.Country))
                     .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                     .ToListAsync();
                     filteredIntegrations = lmts.Where(c => arrCountries.Contains(c.Country)).ToList();

                 }
                 else
                 {
                     string[] arrCountries = countries.Split(",");
                     string[] arrOps = operators.Split(",");

                     allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm") && !String.IsNullOrEmpty(y.Result))).Include("Site").Include("User").Include(x => x.User.AspCompany).Include(x => x.Site.Operator)
                         .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                         .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                         .ToListAsync();
                     filteredIntegrations = lmts.Where(c => arrCountries.Contains(c.Country)).Where(c => arrOps.Contains(c.Operator)).ToList();

                 }
             }
         }

         */
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

        // get LMT global usage 
        public async Task<ActionResult> GetSiteIntegrationUsage(string start, string end, string marketArea)
        {
            string[] countries;
            if (marketArea == null)
                countries = await _context.Countries.Select(x => x.Name).ToArrayAsync();
            else
                countries = await _context.Countries.Where(x => x.MA.Equals(marketArea)).Select(x => x.Name).ToArrayAsync();

            List<SiteIntegration> siteIntegrations = null;
            List<IntegrationDetail> lmts = new List<IntegrationDetail>();

            siteIntegrations = await _context.SiteIntegrations.Where(x => x.SiteName != null)
            .Where(x => x.Outcome != null)
            .Where(x => x.Outcome.ToUpper().Equals("SUCCESS"))
            .OrderBy(x => x.DownloadStart).ToListAsync();

            var integrations = siteIntegrations
                .Where(x => Convert.ToDateTime(x.DownloadStart).Date >= Convert.ToDateTime(start).Date && Convert.ToDateTime(x.DownloadStart).Date <= Convert.ToDateTime(end).Date)
                    .GroupBy(x => new
                    {
                        x.SiteName,
                        x.UserId,
                        start = x.DownloadStart != null ? Convert.ToDateTime(x.DownloadStart).Date : Convert.ToDateTime(x.IntegrateEnd).Date
                    });
            foreach (var integration in integrations)
            {
                User user = await this.GetUser(integration.FirstOrDefault().UserId);
                IntegrationDetail visit = new IntegrationDetail();
                visit.SiteName = integration.FirstOrDefault().SiteName;
                visit.Country = integration.FirstOrDefault().CountryName == null ? user.AspCompany.Country.Name : integration.FirstOrDefault().CountryName;
                visit.Outcome = integration.Last().Outcome;
                lmts.Add(visit);

            }
            var groupedIntegrations = lmts.Where(x => countries.Contains(x.Country))
            .Where(x => x.Outcome != null)
            .Where(x => x.Outcome.ToUpper().Equals("SUCCESS"))
            .GroupBy(x => new { x.Country }).Select(y => new
            {
                country = y.Key.Country,
                users = y.Select(i => i.SiteName).Distinct().Count(),
                percent = String.Format("{0:0.00}", ((float)y.Select(i => i.SiteName).Distinct().Count() / (float)lmts.Count()) * 100)

            });

            return new JsonResult(groupedIntegrations);
        }

        public string GetRole(string email)
        {
            var role = _context.AspManagers.Where(x => x.Email.Equals(email)).Select(x => x.Role).FirstOrDefault();
            return role;
        }
        public async Task<User> GetUser(string userId)
        {
            return await _context.Users.Include(x => x.AspCompany).Include(x => x.AspCompany.Country).FirstOrDefaultAsync(x => x.UserId.Equals(userId));
        }
        public async Task<Site> GetSite(string sitename, string country)
        {
            return await _context.Sites.Include(x => x.Operator).FirstOrDefaultAsync(x => x.Name.Equals(sitename) && x.Country.Equals(country));
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
                siteName = site.FirstOrDefault().Name;
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
                            if (visits[i].StartTime >= T.AddHours(8))
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
                if (siterevisit.Count != 0)
                    dict.Add(siteName, siterevisit);
            }

            // order by number of revisits and return first 10 elements
            dict = dict.OrderByDescending(x => x.Value.Values.Sum())
            .ToDictionary(x => x.Key, x => x.Value.OrderByDescending(y => y.Value)
            .ToDictionary(y => y.Key, y => y.Value))
            .Take(10).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new JsonResult(dict);

        }

        // IMK functions Pass/Fail status 
        public async Task<ActionResult> GetCommandStatus(string start, string end, string countries, string operators)
        {
            Dictionary<string, Dictionary<string, int>> returnList = new Dictionary<string, Dictionary<string, int>>();
            string[] alarmCommands = new string[] { "vswr", "rssi_umts", "rssi-nr", "rssi-lte EUtranCellFDD", "rssi-lte EUtranCellTDD", "alarm" };

            List<SiteVisit> visitLogs = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                visitLogs = await _context.SiteVisits.Include(x => x.Logs.Where(y => alarmCommands.Contains(y.Command)))
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();

            }
            else
            {
                if (operators == null)
                {

                    string[] arrCountries = countries.Split(",");
                    visitLogs = await _context.SiteVisits.Include("Site").Include(x => x.Logs.Where(y => alarmCommands.Contains(y.Command))).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    visitLogs = await _context.SiteVisits.Include("Site").Include(x => x.Logs.Where(y => alarmCommands.Contains(y.Command))).Include(x => x.Site.Operator)
                    .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();

                }
            }


            Dictionary<string, int> pCommands = new Dictionary<string, int>(); //passed
            Dictionary<string, int> fCommands = new Dictionary<string, int>(); //failed


            var check1 = new List<Object>();

            foreach (var site in visitLogs)
            {
                foreach (var logs in site.Logs)
                {
                    if (logs.Result != null)
                    {
                        var command = logs.Command;
                        dynamic results = JsonConvert.DeserializeObject(logs.Result);
                        var passed = 0;
                        if (results != null)
                        {
                            switch (command)
                            {
                                case "vswr":
                                    foreach (var result in results)
                                    {
                                        String status = result.STATUS;
                                        if (status.Equals("PASSED"))
                                            passed = 1;
                                        else if (status.Equals("FAILED"))
                                        {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if (passed == 1)
                                        if (pCommands.ContainsKey("vswr"))
                                            pCommands["vswr"]++;
                                        else
                                            pCommands.Add("vswr", 1);

                                    else
                                        if (fCommands.ContainsKey("vswr"))
                                        fCommands["vswr"]++;
                                    else
                                        fCommands.Add("vswr", 1);

                                    break;

                                case "rssi_umts":
                                    foreach (var result in results)
                                    {
                                        double rssi;
                                        if (result.RSSI == null)
                                            passed = 0;
                                        else
                                        {
                                            bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                            if (isValue == true && rssi <= -100)
                                                passed = 1;
                                            else if (isValue == false || rssi > -100)
                                            {
                                                passed = 0;
                                                break;
                                            }
                                        }
                                    }
                                    if (passed == 1)
                                        if (pCommands.ContainsKey("umts"))
                                            pCommands["umts"]++;
                                        else
                                            pCommands.Add("umts", 1);

                                    else
                                        if (fCommands.ContainsKey("umts"))
                                        fCommands["umts"]++;
                                    else
                                        fCommands.Add("umts", 1);

                                    break;

                                case "rssi-lte EUtranCellFDD":
                                    foreach (var result in results)
                                    {
                                        double rssi;
                                        bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                        if (isValue == true && rssi <= -110)
                                            passed = 1;
                                        else if (isValue == false || rssi > -110)
                                        {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if (passed == 1)
                                        if (pCommands.ContainsKey("fdd"))
                                            pCommands["fdd"]++;
                                        else
                                            pCommands.Add("fdd", 1);

                                    else
                                        if (fCommands.ContainsKey("fdd"))
                                        fCommands["fdd"]++;
                                    else
                                        fCommands.Add("fdd", 1);

                                    break;

                                case "rssi-lte EUtranCellTDD":
                                    foreach (var result in results)
                                    {
                                        double rssi;
                                        bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                        if (isValue == true && rssi <= -110)
                                            passed = 1;
                                        else if (isValue == false || rssi > -110)
                                        {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if (passed == 1)
                                        if (pCommands.ContainsKey("tdd"))
                                            pCommands["tdd"]++;
                                        else
                                            pCommands.Add("tdd", 1);

                                    else
                                        if (fCommands.ContainsKey("tdd"))
                                        fCommands["tdd"]++;
                                    else
                                        fCommands.Add("tdd", 1);

                                    break;

                                case "rssi-nr":
                                    foreach (var result in results)
                                    {
                                        double rssi;
                                        bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                        if (isValue == true && rssi <= -110)
                                            passed = 1;
                                        else if (isValue == false || rssi > -110)
                                        {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if (passed == 1)
                                        if (pCommands.ContainsKey("nr"))
                                            pCommands["nr"]++;
                                        else
                                            pCommands.Add("nr", 1);

                                    else
                                        if (fCommands.ContainsKey("nr"))
                                        fCommands["nr"]++;
                                    else
                                        fCommands.Add("nr", 1);

                                    break;

                                case "alarm":
                                    foreach (var result in results)
                                    {
                                        String description = result.DESCRIPTION;
                                        if (description.Equals(""))
                                            passed = 1;
                                        else if (!description.Equals(""))
                                        {
                                            passed = 0;
                                            break;
                                        }
                                    }
                                    if (passed == 1)
                                        if (pCommands.ContainsKey("alarm"))
                                            pCommands["alarm"]++;
                                        else
                                            pCommands.Add("alarm", 1);

                                    else
                                        if (fCommands.ContainsKey("alarm"))
                                        fCommands["alarm"]++;
                                    else
                                        fCommands.Add("alarm", 1);

                                    break;
                            }
                        }

                    }
                }
            }
            returnList.Add("passed", pCommands);
            returnList.Add("failed", fCommands);
            return new JsonResult(returnList);

        }


        // Get LMT Site Integrations
        public async Task<ActionResult> GetSiteIntegrations(string start, string end, string countries, string operators)
        {
            List<SiteIntegration> siteIntegrations = null;
            List<IntegrationDetail> lmts = new List<IntegrationDetail>();

            siteIntegrations = await _context.SiteIntegrations.Where(x => x.SiteName != null)
            //.Where(x => x.DownloadStart != null)
            //.Where(x => Convert.ToDateTime(x.DownloadStart).Date >= Convert.ToDateTime(start).Date && Convert.ToDateTime(x.DownloadStart).Date <= Convert.ToDateTime(end).Date)
            .OrderBy(x => x.DownloadStart).ToListAsync();

            var integrations = siteIntegrations.GroupBy(x => new
            {
                x.SiteName,
                x.UserId,
                start = x.DownloadStart != null ? Convert.ToDateTime(x.DownloadStart).Date : Convert.ToDateTime(x.IntegrateEnd).Date
            });
            foreach (var integration in integrations)
            {

                User user = await this.GetUser(integration.FirstOrDefault().UserId);
                Site site = await this.GetSite(integration.FirstOrDefault().SiteName, integration.FirstOrDefault().CountryName);

                IntegrationDetail visit = new IntegrationDetail();
                visit.SiteName = integration.FirstOrDefault().SiteName;
                visit.Country = integration.FirstOrDefault().CountryName == null ? user.AspCompany.Country.Name : integration.FirstOrDefault().CountryName;
                visit.User = user.Name;
                visit.Asp = user.AspCompany.Name;
                visit.Operator = site == null ? null : site.Operator.Name;
                visit.DownloadStart = integration.FirstOrDefault().DownloadStart;
                visit.DownloadEnd = integration.Last().DownloadEnd;
                visit.IntegrateStart = integration.FirstOrDefault().IntegrateStart;
                visit.IntegrateEnd = integration.Last().IntegrateEnd;
                visit.Outcome = integration.Last().Outcome;
                visit.IntegrationTime = String.IsNullOrEmpty(visit.DownloadStart) || String.IsNullOrEmpty(visit.IntegrateEnd) ?
                    "0 mins" : ((int)(Convert.ToDateTime(visit.IntegrateEnd) - Convert.ToDateTime(visit.DownloadStart)).TotalMinutes).ToString() + " mins";
                lmts.Add(visit);

            }
            List<IntegrationDetail> filteredIntegrations = null;

            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                filteredIntegrations = lmts;
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    filteredIntegrations = lmts.Where(c => arrCountries.Contains(c.Country)).ToList();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    filteredIntegrations = lmts.Where(c => arrCountries.Contains(c.Country)).Where(c => arrOps.Contains(c.Operator)).ToList();
                }
            }

            return new JsonResult(filteredIntegrations.OrderByDescending(x => x.DownloadStart));
        }



        public async Task<ActionResult> GetResolvedFailures(string start, string end, string countries, string operators)
        {
            string[] alarmCommands = new string[] { "vswr", "rssi_umts", "rssi-nr", "rssi-lte EUtranCellFDD", "rssi-lte EUtranCellTDD", "alarm" };
            List<SiteVisit> allVisits = null;

            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => alarmCommands.Contains(y.Command))).Include("Site").Include("User").Include(x => x.User.AspCompany)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .Where(x => x.Logs.Any(x => alarmCommands.Contains(x.Command)))
                .ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");

                    allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => alarmCommands.Contains(y.Command))).Include("Site").Include("User").Include(x => x.User.AspCompany).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .Where(x => x.Logs.Any(x => alarmCommands.Contains(x.Command)))
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => alarmCommands.Contains(y.Command))).Include("Site").Include("User").Include(x => x.User.AspCompany).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                        .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                        .Where(x => x.Logs.Any(x => alarmCommands.Contains(x.Command)))
                        .ToListAsync();
                }
            }

            var visitDetails = allVisits.OrderBy(y => y.StartTime).Where(y => y.Site.Name != null).GroupBy(x => new { x.Site.Name, x.User.UserId, x.StartTime.Date }).ToList();

            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, int> alarmTypes = new Dictionary<string, int>();

            foreach (var visit in visitDetails)
            {

                Dictionary<string, string> commands = new Dictionary<string, string>();
                List<string> alarms = new List<string>();
                var initialAlarmTime = visit.FirstOrDefault().StartTime;
                foreach (var session in visit)
                {   //session details
                    foreach (var log in session.Logs)
                    {
                        //////////// Alarms
                        List<string> currentAlarms = new List<string>();

                        if (log.Command.Equals("alarm"))
                        {
                            dynamic results = null;

                            if (!log.Result.Equals("null"))
                            {
                                results = JsonConvert.DeserializeObject(log.Result);
                                if (results != null)
                                {
                                    foreach (var alarm in results)
                                    {
                                        string alarmType = (alarm.DESCRIPTION).ToString().Split(".")[0];

                                        if (!commands.ContainsKey(log.Command)) //first alarm attempt
                                        {
                                            if (!alarms.Contains(alarmType)) alarms.Add(alarmType);
                                        }
                                        if (alarmTypes.ContainsKey(alarmType)) alarmTypes[alarmType]++;
                                        else alarmTypes[alarmType] = 1;
                                        if (!currentAlarms.Contains(alarmType)) currentAlarms.Add(alarmType);

                                    }
                                }
                            }

                            IEnumerable<string> clearedAlarms = alarms.Except(currentAlarms);
                            IEnumerable<string> newAlarms = currentAlarms.Except(alarms);

                            // update the alarms left
                            foreach (var item in newAlarms) alarms.Add(item);

                            if (clearedAlarms.Count() != 0)
                            {
                                commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Resolved", Duration = (session.StartTime - initialAlarmTime).TotalMinutes });
                            }
                            // else if (newAlarms.Count() > 0)
                            // {
                            //     commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Failed", Time = session.StartTime });
                            // }
                            else
                            {
                                commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Failed", Time = session.StartTime });
                            }
                            alarms.RemoveAll(a => clearedAlarms.Contains(a));
                            //commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Passed", Time = session.StartTime });
                        }

                        else
                        {
                            if (log.Result != null)
                            {
                                //check if command is in dictionary then update the status, else add new status
                                var passed = CommandPassed(log.Command, log.Result);
                                if (commands.ContainsKey(log.Command))
                                {
                                    dynamic firstAttempt = JsonConvert.DeserializeObject(commands[log.Command]);
                                    // to be resolved check if command previously failed and now passed
                                    if (commands[log.Command].Contains("Failed") && passed)
                                        commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Resolved", Duration = (session.StartTime - Convert.ToDateTime(firstAttempt.Time)).TotalMinutes });

                                }
                                else commands[log.Command] = JsonConvert.SerializeObject(new { Status = (passed ? "Passed" : "Failed"), Time = session.StartTime });
                            }
                        }
                    }

                }
                if (commands.Count() != 0)
                    list.Add(commands);
            }

            Dictionary<string, List<string>> details = new Dictionary<string, List<string>>();
            foreach (var item in list)
            {
                foreach (var log in item)
                {
                    var command = log.Key;
                    var status = log.Value;
                    if (details.ContainsKey(command))
                    {
                        details[command].Add(status);
                    }
                    else
                    {
                        details.Add(command, new List<string> { status });
                    }
                }

            }
            Dictionary<string, int> vpassed = new Dictionary<string, int>(); //passed per visit
            Dictionary<string, int> vfailed = new Dictionary<string, int>(); //failed per visit
            Dictionary<string, int> resolved = new Dictionary<string, int>(); //resolved per visit
            Dictionary<string, int> resolvedtime = new Dictionary<string, int>(); //avg resolution time
            Dictionary<string, int> median = new Dictionary<string, int>(); //median for resolution time

            foreach (var i in details)
            {
                vpassed.Add(i.Key, i.Value.Where(x => x.Contains("Passed") || x.Contains("Resolved")).Count());
                vfailed.Add(i.Key, i.Value.Where(x => x.Contains("Failed")).Count());
                resolved.Add(i.Key, i.Value.Where(x => x.Contains("Resolved")).Count());

                List<double> duration = new List<double>();
                foreach (var j in i.Value.Where(x => x.Contains("Resolved")))
                {
                    dynamic info = JsonConvert.DeserializeObject(j);
                    var time = info.Duration;
                    duration.Add(Convert.ToDouble(time));

                }
                resolvedtime.Add(i.Key, duration.Count() == 0 ? 0 : (int)duration.Average());
                median.Add(i.Key, duration.Count() == 0 ? 0 : (int)duration.Median());

            }
            double totalTypes = alarmTypes.Sum(x => x.Value);
            Dictionary<string, int> fieldAlarms = new Dictionary<string, int>();
            Dictionary<string, int> remoteAlarms = new Dictionary<string, int>();
            Dictionary<string, int> miscAlarms = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> entry in alarmTypes)
            {
                // alarmTypes[entry.Key] = (int)Math.Round((double)(entry.Value / totalTypes) * 100);
                if (GetAlarmType(entry.Key).Equals("Field"))
                {
                    if (fieldAlarms.ContainsKey(entry.Key)) fieldAlarms[entry.Key]++;
                    else fieldAlarms[entry.Key] = entry.Value;
                }
                else if (GetAlarmType(entry.Key).Equals("Remote"))
                {
                    if (remoteAlarms.ContainsKey(entry.Key)) remoteAlarms[entry.Key]++;
                    else remoteAlarms[entry.Key] = entry.Value;
                }
                else
                {
                    if (miscAlarms.ContainsKey(entry.Key)) miscAlarms[entry.Key]++;
                    else miscAlarms[entry.Key] = entry.Value;
                }

            }

            Dictionary<string, Dictionary<string, int>> returnList = new Dictionary<string, Dictionary<string, int>>();
            returnList.Add("passed_per_visit", vpassed);
            returnList.Add("failed_per_visit", vfailed);
            returnList.Add("resolved_per_visit", resolved);
            returnList.Add("avg_resolution", resolvedtime);
            returnList.Add("median_resolution", median);
            returnList.Add("alarm_types", alarmTypes.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value));
            returnList.Add("field_alarms", fieldAlarms.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value));
            returnList.Add("remote_alarms", remoteAlarms.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value));
            returnList.Add("misc_alarms", miscAlarms.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value));



            return new JsonResult(returnList);
        }


        public bool CommandPassed(string command, string outcome)
        {
            dynamic results = JsonConvert.DeserializeObject(outcome);
            if (results != null)
            {
                var passed = 0;

                switch (command)
                {
                    case "vswr":
                        foreach (var result in results)
                        {
                            String status = result.STATUS;
                            if (status.Equals("PASSED"))
                                passed = 1;
                            else if (status.Equals("FAILED"))
                            {
                                passed = 0;
                                break;
                            }
                        }
                        if (passed == 1)
                            return true;
                        else
                            return false;

                    case "rssi_umts":
                        foreach (var result in results)
                        {
                            double rssi;
                            if (result.RSSI == null)
                                passed = 0;
                            else
                            {
                                bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                                if (isValue == true && rssi <= -100)
                                    passed = 1;
                                else if (isValue == false || rssi > -100)
                                {
                                    passed = 0;
                                    break;
                                }
                            }
                        }
                        if (passed == 1)
                            return true;
                        else
                            return false;

                    case "rssi-lte EUtranCellFDD":
                        foreach (var result in results)
                        {
                            double rssi;
                            bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                            if (isValue == true && rssi <= -110)
                                passed = 1;
                            else if (isValue == false || rssi > -110)
                            {
                                passed = 0;
                                break;
                            }
                        }
                        if (passed == 1)
                            return true;
                        else
                            return false;

                    case "rssi-lte EUtranCellTDD":
                        foreach (var result in results)
                        {
                            double rssi;
                            bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                            if (isValue == true && rssi <= -110)
                                passed = 1;
                            else if (isValue == false || rssi > -110)
                            {
                                passed = 0;
                                break;
                            }
                        }
                        if (passed == 1)
                            return true;
                        else
                            return false;

                    case "rssi-nr":
                        foreach (var result in results)
                        {
                            double rssi;
                            bool isValue = double.TryParse((result.RSSI).ToString(), out rssi);
                            if (isValue == true && rssi <= -110)
                                passed = 1;
                            else if (isValue == false || rssi > -110)
                            {
                                passed = 0;
                                break;
                            }
                        }
                        if (passed == 1)
                            return true;
                        else
                            return false;

                    case "alarm":
                        foreach (var result in results)
                        {
                            String description = result.DESCRIPTION;
                            if (description.Equals(""))
                                passed = 1;
                            else if (!description.Equals(""))
                            {
                                passed = 0;
                                break;
                            }
                        }
                        if (passed == 1)
                            return true;
                        else
                            return false;
                }
            }
            return false;
        }



        //////////////////////////////////////////////////////////////////// Alarms ////////////////////////////////////////////////////////////////////////////

        //get site visits T = 8 hours
        public async Task<ActionResult> GetResolutionTimes(string start, string end) //TODO
        {

            var allVisits = await _context.SiteVisits.Include("Site").Include("User").Include(x => x.User.AspCompany)
                .Include(x => x.Site.Operator).Include(x => x.Logs)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                .ToListAsync();

            var visitDetails = allVisits.OrderBy(y => y.StartTime).Where(y => y.Site.Name != null).GroupBy(x => new { x.Site.Name, x.User.UserId }).Select(y => new
            {
                siteName = y.Key.Name,
                country = y.Select(i => i.Site.Country),
                user = y.Select(i => i.User.Name),
                phone = y.Select(i => i.User.Phone),
                email = y.Select(i => i.User.Email),
                androidVersion = y.Select(i => i.AppVersion),
                rpVersion = y.Select(i => i.RPIVersion),
                asp = y.Select(i => i.User.AspCompany.Name),
                date = y.Select(i => i.StartTime),
                logs = y.Select(i => i.Logs)
            });
            List<VisitDetail> uniqueVisits = new List<VisitDetail>();
            Dictionary<string, DateTime> siterevisit = new Dictionary<string, DateTime>();

            foreach (var vd in visitDetails)
            {
                var revisit = false;
                if (siterevisit.ContainsKey(vd.siteName))
                {
                    var T = siterevisit[vd.siteName];
                    if (vd.date.FirstOrDefault() >= T.AddHours(8))
                    {
                        revisit = true;
                        siterevisit[vd.siteName] = vd.date.FirstOrDefault();
                    }
                }
                else
                    siterevisit.Add(vd.siteName, vd.date.FirstOrDefault());

                if (vd.date.Count() > 1)
                {
                    var d = vd.date.ToArray();
                    var T = d[0];
                    VisitDetail v1 = new VisitDetail();
                    v1.SiteName = vd.siteName;
                    v1.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Phone = vd.phone.FirstOrDefault();
                    v1.Email = vd.email.FirstOrDefault();
                    v1.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                    v1.ASP = vd.asp.FirstOrDefault();
                    v1.Date = d[0].Date.ToString("yyyy-MM-dd");
                    v1.IsRevisit = revisit;
                    uniqueVisits.Add(v1);



                    for (var i = 1; i < d.Length; i++)
                    {
                        if (d[i] >= T.AddHours(8))
                        {
                            VisitDetail v = new VisitDetail();
                            v.SiteName = vd.siteName;
                            v.Country = vd.country.FirstOrDefault();
                            v.User = vd.user.FirstOrDefault();
                            v.Phone = vd.phone.FirstOrDefault();
                            v.Email = vd.email.FirstOrDefault();
                            v.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                            v.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                            v.ASP = vd.asp.FirstOrDefault();
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
                    v1.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Phone = vd.phone.FirstOrDefault();
                    v1.Email = vd.email.FirstOrDefault();
                    v1.AppVersion = ((double)vd.androidVersion.FirstOrDefault()).ToString("0.00");
                    v1.RpiVersion = ((double)vd.rpVersion.FirstOrDefault()).ToString("0.00");
                    v1.ASP = vd.asp.FirstOrDefault();
                    v1.Date = vd.date.FirstOrDefault().Date.ToString("yyyy-MM-dd");
                    v1.IsRevisit = revisit;
                    uniqueVisits.Add(v1);
                }

            }
            return new JsonResult(uniqueVisits);

        }

        public async Task<ActionResult> GetCountriesResolutionTimes(string start, string end, string marketArea)
        {
            string[] alarmCommands = new string[] { "alarm" };
            var countries = await this.GetIMKCountriesByMA(marketArea);
            var c = countries.Select(c => c.Country).Distinct().ToList();

            var allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm"))).Include("Site")
            .Include("User")
            .Where(x => c.Contains(x.Site.Country))
            .Where(x => x.Logs.Any(x => x.Command.Equals("alarm")))
            .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
            .ToListAsync();

            var visitDetails = allVisits.OrderBy(y => y.StartTime).Where(y => y.Site.Name != null)
            .Where(x => x.Logs.Any(y => !(GetAlarmType(y.Result)).Equals("Remote")))
            .GroupBy(x => new { x.Site, x.User.UserId }).Select(y => new
            {
                site = y.Key.Site,
                country = y.Select(i => i.Site.Country),
                user = y.Select(i => i.User),
                date = y.Select(i => i.StartTime),
                logs = y.SelectMany(i => i.Logs)
            });
            List<SiteVisit> uniqueVisits = new List<SiteVisit>();

            foreach (var vd in visitDetails)
            {
                if (vd.date.Count() > 1)
                {
                    var newLogs = new List<Log>();
                    var d = vd.date.ToArray();
                    var sv = vd.site.SiteVisits.ToArray();
                    var T = d[0];
                    SiteVisit v1 = new SiteVisit();
                    v1.Site = vd.site;
                    v1.Site.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.StartTime = vd.date.FirstOrDefault();
                    newLogs = sv[0].Logs.ToList();

                    for (var i = 1; i < d.Length; i++)
                    {
                        if (d[i] >= T.AddHours(8))
                        {
                            SiteVisit v = new SiteVisit();
                            v.Site = vd.site;
                            v.Site.Country = vd.country.FirstOrDefault();
                            v.User = vd.user.FirstOrDefault();
                            v.Logs = sv[i].Logs;
                            v.StartTime = sv[i].StartTime;
                            uniqueVisits.Add(v);
                        }
                        else
                        {
                            foreach (Log log in sv[i].Logs)
                                newLogs.Add(log);
                        }
                        T = d[i];
                    }
                    v1.Logs = newLogs;
                    uniqueVisits.Add(v1);
                }
                else
                {
                    SiteVisit v1 = new SiteVisit();
                    v1.Site = vd.site;
                    v1.Site.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Logs = vd.logs;
                    v1.StartTime = vd.date.FirstOrDefault();
                    uniqueVisits.Add(v1);
                }

            }
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, int> alarmTypes = new Dictionary<string, int>();

            foreach (var visit in uniqueVisits.OrderBy(x => x.StartTime))
            {
                Dictionary<string, string> commands = new Dictionary<string, string>();
                List<string> alarms = new List<string>();
                var initialAlarmTime = visit.Logs.OrderBy(x => x.TimeOfAction).FirstOrDefault().TimeOfAction;

                foreach (var log in visit.Logs.OrderBy(x => x.TimeOfAction))
                {
                    //////////// Alarms
                    List<string> currentAlarms = new List<string>();

                    if (log.Command.Equals("alarm"))
                    {
                        dynamic results = null;

                        if (!log.Result.Equals("null"))
                        {
                            results = JsonConvert.DeserializeObject(log.Result);

                            foreach (var alarm in results)
                            {
                                string alarmType = (alarm.DESCRIPTION).ToString().Split(".")[0];

                                if (!commands.ContainsKey(log.Command)) //first alarm attempt
                                {
                                    if (!alarms.Contains(alarmType)) alarms.Add(alarmType);
                                }
                                if (alarmTypes.ContainsKey(alarmType)) alarmTypes[alarmType]++;
                                else alarmTypes[alarmType] = 1;
                                if (!currentAlarms.Contains(alarmType)) currentAlarms.Add(alarmType);

                            }
                        }

                        IEnumerable<string> clearedAlarms = alarms.Except(currentAlarms);
                        IEnumerable<string> newAlarms = currentAlarms.Except(alarms);

                        // update the alarms left
                        alarms.RemoveAll(a => clearedAlarms.Contains(a));
                        foreach (var item in newAlarms) alarms.Add(item);
                        List<string> fieldAlarmsOnly = new List<string>();
                        alarmTypes.ToList().ForEach(alarmToGetType =>
                        {
                            if (GetAlarmType(alarmToGetType.Key).Equals("Field"))
                            {
                                if (alarms.Count == 0)
                                {
                                    var firstLog = visit.Logs.FirstOrDefault(item => !String.IsNullOrEmpty(item.Result) && item.Result.Contains(alarmToGetType.Key));
                                    initialAlarmTime = firstLog != null ? firstLog.TimeOfAction : initialAlarmTime;
                                }
                                fieldAlarmsOnly.Add(alarmToGetType.Key);
                            }
                        });
                        if (alarms.Count == 0 && fieldAlarmsOnly.Any())
                        {
                            commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Resolved", Duration = (log.TimeOfAction - initialAlarmTime).TotalMinutes, Country = visit.Site.Country });
                        }

                    }

                }


                if (commands.Count() != 0)
                    list.Add(commands);
            }

            Dictionary<string, List<string>> details = new Dictionary<string, List<string>>();
            foreach (var item in list)
            {
                foreach (var log in item)
                {
                    var command = log.Key;
                    var status = log.Value;
                    if (details.ContainsKey(command))
                    {
                        details[command].Add(status);
                    }
                    else
                    {
                        details.Add(command, new List<string> { status });
                    }
                }

            }
            Dictionary<string, int> resolved = new Dictionary<string, int>(); //resolved per visit
            Dictionary<string, int> resolvedtime = new Dictionary<string, int>(); //avg resolution time
            Dictionary<string, List<double>> resolvedByCountry = new Dictionary<string, List<double>>();
            Dictionary<string, double> resolvedAvgByCountry = new Dictionary<string, double>();



            foreach (var i in details)
            {
                resolved.Add(i.Key, i.Value.Where(x => x.Contains("Resolved")).Count());
                List<double> duration = new List<double>();
                foreach (var j in i.Value.Where(x => x.Contains("Resolved")))
                {
                    dynamic info = JsonConvert.DeserializeObject(j);
                    double time = info.Duration;
                    string country = info.Country;
                    if (resolvedByCountry.ContainsKey(country))
                        resolvedByCountry[country].Add(time);
                    else
                        resolvedByCountry.Add(country, new List<double> { time });

                }
            }
            foreach (KeyValuePair<string, List<double>> entry in resolvedByCountry)
            {
                resolvedAvgByCountry.Add(entry.Key, (entry.Value).Count() == 0 ? 0 : (int)(entry.Value).Average());
            }

            return new JsonResult(resolvedAvgByCountry.OrderBy(s => s.Value));

        }

        public async Task<ActionResult> GetAlarmAnalysis(string start, string end, string countries, string operators)
        {
            string[] alarmCommands = new string[] { "alarm" };

            List<SiteVisit> allVisits = null;
            if (countries == null)
                return new JsonResult(null);

            else if (countries == "all")
            {
                allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm"))).Include("Site").Include("User").Include(x => x.User.AspCompany)
                .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                //.Where(x => x.Logs.Any(x => x.Command.Equals("alarm")))
                .ToListAsync();
            }
            else
            {
                if (operators == null)
                {
                    string[] arrCountries = countries.Split(",");
                    allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm"))).Include("Site").Include("User").Include(x => x.User.AspCompany).Where(c => arrCountries.Contains(c.Site.Country))
                    .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                    .ToListAsync();
                }
                else
                {
                    string[] arrCountries = countries.Split(",");
                    string[] arrOps = operators.Split(",");

                    allVisits = await _context.SiteVisits.Include(x => x.Logs.Where(y => y.Command.Equals("alarm"))).Include("Site").Include("User").Include(x => x.User.AspCompany).Include(x => x.Site.Operator)
                        .Where(c => arrCountries.Contains(c.Site.Country)).Where(c => arrOps.Contains(c.Site.Operator.Name))
                        .Where(x => x.StartTime.Date >= Convert.ToDateTime(start).Date && x.StartTime.Date <= Convert.ToDateTime(end).Date)
                        .ToListAsync();
                }
            }

            var visitDetails = allVisits.OrderBy(y => y.StartTime).Where(y => y.Site.Name != null)
            //.Where(x => x.Logs.Any(y => !(GetAlarmType(y.Result)).Equals("Remote")))
            .GroupBy(x => new { x.Site, x.User?.UserId, x.StartTime.Date }).Select(y => new
            {
                site = y.Key.Site,
                country = y.Select(i => i.Site.Country),
                user = y.Select(i => i.User),
                date = y.Select(i => i.StartTime),
                logs = y.Select(i => i.Logs)
            });
            List<SiteVisit> uniqueVisits = new List<SiteVisit>();

            foreach (var vd in visitDetails)
            {
                if (vd.date.Count() > 1)
                {
                    var _a = new List<Log>(); ;

                    vd.logs.ToList().ForEach(l =>
                    {
                        l.ToList().ForEach(sl =>
                        {
                            _a.Add(sl);
                        });
                    });
                    var newLogs = new List<Log>();
                    var d = vd.date.ToArray();
                    var sv = vd.site.SiteVisits.ToArray();
                    var T = d[0];
                    SiteVisit v1 = new SiteVisit();
                    v1.Site = vd.site;
                    v1.Site.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.StartTime = vd.date.FirstOrDefault();
                    newLogs = sv[0].Logs.ToList();
                    v1.Logs = _a;


                    for (var i = 1; i < d.Length; i++)
                    {
                        if (d[i] >= T.AddHours(8))
                        {
                            var a = new List<Log>(); ;

                            vd.logs.ToList().ForEach(l =>
                            {
                                l.ToList().ForEach(sl =>
                                {
                                    a.Add(sl);
                                });
                            });
                            SiteVisit v = new SiteVisit();
                            v.Site = vd.site;
                            v.Site.Country = vd.country.FirstOrDefault();
                            v.User = vd.user.FirstOrDefault();
                            //v.Logs = sv[i].Logs;
                            v.Logs = a;
                            v.StartTime = sv[i].StartTime;
                            uniqueVisits.Add(v);
                        }
                        else
                        {
                            foreach (Log log in sv[i].Logs)
                                newLogs.Add(log);
                        }
                        T = d[i];
                    }
                    // v1.Logs = newLogs;
                    //v1.Logs = a
                    uniqueVisits.Add(v1);
                }
                else
                {
                    var a = new List<Log>(); ;

                    vd.logs.ToList().ForEach(l =>
                    {
                        l.ToList().ForEach(sl =>
                        {
                            a.Add(sl);
                        });
                    });
                    SiteVisit v1 = new SiteVisit();
                    v1.Site = vd.site;
                    v1.Site.Country = vd.country.FirstOrDefault();
                    v1.User = vd.user.FirstOrDefault();
                    v1.Logs = a;
                    v1.StartTime = vd.date.FirstOrDefault();
                    uniqueVisits.Add(v1);
                }

            }
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, int> alarmTypes = new Dictionary<string, int>();

            foreach (var visit in uniqueVisits.OrderBy(x => x.StartTime))
            {
                Dictionary<string, string> commands = new Dictionary<string, string>();
                List<string> alarms = new List<string>();
                IEnumerable<string> clearedAlarms = new List<string>();
                var initialAlarmTime = visit.Logs?.OrderBy(x => x.TimeOfAction)?.FirstOrDefault()?.TimeOfAction;
                foreach (var log in visit.Logs.OrderBy(x => x.TimeOfAction))
                {
                    //////////// Alarms
                    List<string> currentAlarms = new List<string>(); //if alarms null then list is empty

                    if (log.Command.Equals("alarm"))
                    {
                        dynamic results = null;


                        results = JsonConvert.DeserializeObject(log.Result);
                        if (results != null)
                        {
                            foreach (var alarm in results)
                            {
                                string alarmType = (alarm.DESCRIPTION).ToString().Split(".")[0];
                                if (GetAlarmType(alarmType).Equals("Field"))
                                {
                                    if (!commands.ContainsKey(log.Command)) //first alarm attempt
                                    {
                                        if (!alarms.Contains(alarmType)) alarms.Add(alarmType);
                                    }
                                    if (alarmTypes.ContainsKey(alarmType)) alarmTypes[alarmType]++;
                                    else alarmTypes[alarmType] = 1;
                                    if (!currentAlarms.Contains(alarmType)) currentAlarms.Add(alarmType);
                                    initialAlarmTime = log.TimeOfAction;
                                }
                            }
                        }

                        clearedAlarms = alarms.Except(currentAlarms);
                        //   alarms = alarms.Except(clearedAlarms.ToList()).ToList();
                        IEnumerable<string> newAlarms = currentAlarms.Except(alarms);

                        // update the alarms left
                        //alarms.RemoveAll(a => clearedAlarms.Contains(a));
                        // foreach (var item in newAlarms) alarms.Add(item);
                        //var _alarms = new List<string>();
                        // List<string> fieldAlarmsOnly = new List<string>();
                        if (alarms.Any() && !clearedAlarms.Any())
                        {
                            initialAlarmTime = log.TimeOfAction;
                        }
                        if (clearedAlarms.Any())
                        {
                            commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Resolved", Duration = (log.TimeOfAction - initialAlarmTime)?.TotalMinutes, Country = visit.Site.Country });
                            alarms.RemoveAll(a => clearedAlarms.Contains(a));
                        }
                        else
                        {

                            if (alarms.Any())
                            {
                                commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Passed", Duration = 0, Country = visit.Site.Country });
                            }
                        }
                        /*if (alarms.Any())
                        {

                            commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Failed", Time = visit.StartTime, Country = visit.Site.Country });
                            list.Add(commands);
                        }
                        if (!clearedAlarms.Any() && !alarms.Any())
                        {
                            commands[log.Command] = JsonConvert.SerializeObject(new { Status = "Passed", Duration = 0, Country = visit.Site.Country });
                            list.Add(commands);
                        }*/

                        foreach (var item in newAlarms) alarms.Add(item);

                        //if (commands.Count() != 0)
                        // list.Add(commands);
                    }

                }
                if (commands.Count() != 0)
                    list.Add(commands);

            }

            Dictionary<string, List<string>> details = new Dictionary<string, List<string>>();
            foreach (var item in list)
            {
                foreach (var log in item)
                {
                    var command = log.Key;
                    var status = log.Value;
                    if (details.ContainsKey(command))
                    {
                        details[command].Add(status);
                    }
                    else
                    {
                        details.Add(command, new List<string> { status });
                    }
                }

            }
            Dictionary<string, int> vpassed = new Dictionary<string, int>(); //passed per visit
            Dictionary<string, int> vfailed = new Dictionary<string, int>(); //failed per visit
            Dictionary<string, int> resolved = new Dictionary<string, int>(); //resolved per visit
            Dictionary<string, int> resolvedtime = new Dictionary<string, int>(); //avg resolution time
            Dictionary<string, int> median = new Dictionary<string, int>(); //median for resolution time


            foreach (var i in details)
            {
                resolved.Add(i.Key, i.Value.Where(x => x.Contains("Resolved")).Count());
                vfailed.Add(i.Key, i.Value.Where(x => x.Contains("Failed")).Count());
                vpassed.Add(i.Key, i.Value.Where(x => x.Contains("Passed")).Count());

                List<double> duration = new List<double>();
                foreach (var j in i.Value.Where(x => x.Contains("Resolved")))
                {
                    dynamic info = JsonConvert.DeserializeObject(j);
                    var time = info.Duration;
                    duration.Add(Convert.ToDouble(time));

                }
                resolvedtime.Add(i.Key, duration.Count() == 0 ? 0 : (int)duration.Average());
                median.Add(i.Key, duration.Count() == 0 ? 0 : (int)duration.Median());

            }
            double totalTypes = alarmTypes.Sum(x => x.Value);
            Dictionary<string, int> fieldAlarms = new Dictionary<string, int>();
            Dictionary<string, int> remoteAlarms = new Dictionary<string, int>();
            Dictionary<string, int> miscAlarms = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> entry in alarmTypes)
            {
                alarmTypes[entry.Key] = (int)Math.Round((double)(entry.Value / totalTypes) * 100);
                var type = (entry.Key).Split(",")[0];
                if (GetAlarmType(type).Equals("Field"))
                {
                    if (fieldAlarms.ContainsKey(type)) fieldAlarms[type]++;
                    else fieldAlarms[type] = entry.Value;
                }
                else if (GetAlarmType(type).Equals("Remote"))
                {
                    if (remoteAlarms.ContainsKey(type)) remoteAlarms[type]++;
                    else remoteAlarms[type] = entry.Value;
                }
                else
                {
                    if (miscAlarms.ContainsKey(type)) miscAlarms[type]++;
                    else miscAlarms[type] = entry.Value;
                }

            }
            Dictionary<string, Dictionary<string, int>> returnList = new Dictionary<string, Dictionary<string, int>>();
            returnList.Add("failed_per_visit", vfailed);
            returnList.Add("passed_per_visit", vpassed);
            returnList.Add("resolved_per_visit", resolved);
            returnList.Add("avg_resolution", resolvedtime);
            return new JsonResult(returnList);
        }

        /* public async Task<ActionResult> newGetAlarmAnalysis(string start, string end, string countries, string operators)
         {

         }*/


        public async Task<Log> GetLatestAlarm(string start, string end, int siteId)
        {

            var alarms = await _context.Logs.Include(x => x.SiteVisit).Include(x => x.SiteVisit.Site)
            .Where(x => x.SiteVisit.StartTime.Date >= Convert.ToDateTime(start).Date && x.SiteVisit.StartTime < Convert.ToDateTime(end))
            .Where(x => x.SiteVisit.Site.SiteId == siteId)
            .Where(x => x.Command.Equals("alarm"))
            .Where(x => !x.Result.Equals("null"))
            .OrderBy(x => x.SiteVisit.StartTime).ToListAsync();

            if (alarms.Count() > 0)
                return alarms.Last();
            else
                return null;
        }


        public string GetAlarmType(string _alarm)
        {
            var alarmData = System.IO.File.ReadAllText("Data/alarms.json");
            dynamic alarms = JsonConvert.DeserializeObject(alarmData);
            var type = "";

            foreach (var alarm in alarms)
            {
                string alarmStatement = alarm.alarm;
                if (alarmStatement.ToUpper().Contains(_alarm.ToUpper()))
                {
                    type = alarm.type;
                }
            }
            return type;

        }

        public async Task<ActionResult> GetAlarmTypes()
        {

            var alarms = await _context.Logs
            .Where(x => x.Command.Equals("alarm"))
            .Where(x => !x.Result.Equals("null"))
            .ToListAsync();

            List<string> alarmTypes = new List<string>();
            foreach (var alarm in alarms)
            {
                if (alarm != null && alarm.Result != null)
                {
                    dynamic results = JsonConvert.DeserializeObject(alarm.Result);
                    foreach (var res in results)
                    {
                        string type = res.DESCRIPTION;
                        if (!alarmTypes.Contains(type))
                            alarmTypes.Add(type);
                    }
                }
            }
            return new JsonResult(alarmTypes);
        }
        public async Task<ActionResult> GetRatings(string start, string end)
        {
            var ratings = await _context.Ratings.Include("User").Where(r => r.Date >= Convert.ToDateTime(start).Date && r.Date < Convert.ToDateTime(end)).ToListAsync();
            ratings.ForEach(e =>
            {
                var country = _IMKHelperservice.geCountryFromAzureMaps(e.Latitude, e.Longitude).Result;
                if (country != null)
                    e.Country = country?.CountryName;
                else
                    e.Country = null;
            });
            return new JsonResult(ratings.OrderByDescending(i => i.Date));
        }

    }



}
