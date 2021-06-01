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
    public class AppRepository : IAppRepository
    {
        private readonly DataContext _context;

        public AppRepository(DataContext dataContext)
        {
            this._context = dataContext;

        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }
        public void Remove<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Site> GetSite(string sitename)
        {
            return await _context.Sites.Include(x => x.SiteVisits).FirstOrDefaultAsync(x => x.Name.Equals(sitename));
        }

        public async Task<User> GetUser(string userId)
        {
            return await _context.Users.Include(x=>x.AspCompany).Include(x=>x.AspCompany.Country).FirstOrDefaultAsync(x => x.UserId.Equals(userId));
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.Email.Equals(email));
        }
        public async Task<Country> GetCountryByName(string country)
        {
            return await _context.Countries.FirstOrDefaultAsync(x => x.Name.Equals(country));
        }

        public async Task<ImkVersion> GetImkVersion(double rpi, double app)
        {
            return await _context.ImkVersions.FirstOrDefaultAsync(x => x.RPIVersion == rpi && x.AppVersion == app);
        }

        public async Task<bool> SaveChanges()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AspCompany> GetAspCompanyByCountry(string aspName, string country)
        {
            return await _context.AspCompanies.FirstOrDefaultAsync(x => x.Name.Equals(aspName) && x.Country.Name.Equals(country));
        }

        public async Task<AspCompany> GetAspCompany(string aspName)
        {
            return await _context.AspCompanies.Include(x => x.Country).FirstOrDefaultAsync(x => x.Name.Equals(aspName));
        }

        public async Task<Country> GetOperatorByCountry(string country)
        {
            return await _context.Countries.Include(x => x.Operators).FirstOrDefaultAsync(x => x.Name.Equals(country));
        }

        public async Task<IEnumerable<Country>> GetCountries()
        {
            return await _context.Countries.Include(x => x.AspCompanies).Include(x => x.Operators).ToListAsync();
        }

        public async Task<ImkVersion> GetLatestImkVersion()
        {
            return await _context.ImkVersions.OrderBy(x => x.DateOfRelease).LastAsync();
        }

        public async Task<IEnumerable<AspManager>> GetAspManagers(string country)
        {
            return await _context.AspManagers.Where(x => x.Country.Equals(country)).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.Include(x => x.AspCompany).Include(x => x.AspCompany.Country).OrderByDescending(x => x.RegisteredAt).ToListAsync();
        }

        public async Task<ActionResult> GetLogs()
        {
            var allLogs = await _context.Logs.Include(x => x.SiteVisit).Include(x => x.SiteVisit.Site)
            .Include(x => x.SiteVisit.User).OrderBy(x => x.SiteVisit.StartTime).ToListAsync();

            var logs = allLogs.GroupBy(x => x.SiteVisit.VisitId).Select(y => new {
                date = y.Select(i => i.SiteVisit.StartTime).Distinct(),
                country = y.Select(i =>i.SiteVisit.Site.Country).Distinct(),
                site = y.Select(i =>i.SiteVisit.Site.Name).Distinct(),
                longitude = y.Select(i => i.Longitude).Distinct(),
                latitude = y.Select(i => i.Latitude).Distinct(),
                rpi = y.Select(i => i.SiteVisit.RPIVersion).Distinct(),
                app = y.Select(i => i.SiteVisit.AppVersion).Distinct(),
                user = y.Select(i => i.SiteVisit.User.Name).Distinct(),
                command = y.Select(i=>i.Command),
                result = y.Select(i => i.Result),
            });
            return new JsonResult(logs);

        }
        public async Task<IEnumerable<AspManager>> GetApprovers()
        {
            return await _context.AspManagers.ToListAsync();
        }

        public async Task<AspManager> GetApprover(string email)
        {
            return await _context.AspManagers.SingleOrDefaultAsync(x => x.Email.Equals(email));
        }



    }
}
