using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Models;
using System.Linq;
using IMK_web.Data;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;

namespace IMK_web.Repository
{
    public class PortalRepository : IPortalRepository
    {
        private readonly DataContext _context;

        public PortalRepository(DataContext dataContext)
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
        public async Task<bool> SaveChanges()
        {
            return await _context.SaveChangesAsync() > 0;
        }
         public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.Include(x => x.AspCompany).Include(x => x.AspCompany.Country)
            .Include(x => x.SiteVisits)
            .OrderByDescending(x => x.RegisteredAt).ToListAsync();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.Email.Equals(email));
        }

        public async Task<Country> GetCountryByName(string country)
        {
            return await _context.Countries.FirstOrDefaultAsync(x => x.Name.Equals(country));
        }
      
        public async Task<IEnumerable<AspManager>> GetApprovers()
        {
            return await _context.AspManagers.OrderBy(x => x.Country).ToListAsync();
        }

        public async Task<AspManager> GetApprover(int id)
        {
            return await _context.AspManagers.SingleOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task<IEnumerable<AspCompany>> GetAspCompanies()
        {
            return await _context.AspCompanies.Include(x => x.Country).ToListAsync();
        }

        public async Task<ActionResult> GetLogs(string start, string end)
        {
            var allLogs = await _context.Logs.Include(x => x.SiteVisit).Include(x => x.SiteVisit.Site).Include(x => x.SiteVisit.User)
            .Where(x => x.SiteVisit.StartTime.Date >= Convert.ToDateTime(start).Date && x.SiteVisit.StartTime.Date <= Convert.ToDateTime(end).Date)
            .OrderBy(x => x.SiteVisit.StartTime).ToListAsync();

            var logs = allLogs.GroupBy(x => x.SiteVisit.VisitId).Select(y => new
            {
                date = y.Select(i => i.SiteVisit.StartTime).Distinct(),
                country = y.Select(i => i.SiteVisit.Site.Country).Distinct(),
                site = y.Select(i => i.SiteVisit.Site.Name).Distinct(),
                longitude = y.Select(i => i.SiteVisit.Site.Longitude).Distinct(),
                latitude = y.Select(i => i.SiteVisit.Site.Latitude).Distinct(),
                rpi = y.Select(i => i.SiteVisit.RPIVersion).Distinct(),
                app = y.Select(i => i.SiteVisit.AppVersion).Distinct(),
                user = y.Select(i => i.SiteVisit.User.Name).Distinct(),
                command = y.Select(i => i.Command),
                result = y.Select(i => i.Result),
            });
            return new JsonResult(logs);
        }

        public async Task<ActionResult> GetFilteredLogs(LogsFilter logsFilter)
        {
            var allLogs = await _context.Logs.Include(x => x.SiteVisit).Include(x => x.SiteVisit.Site).Include(x => x.SiteVisit.User)
            .Where(x => x.SiteVisit.StartTime.Date >= Convert.ToDateTime(logsFilter.StartDate).Date && x.SiteVisit.StartTime.Date <= Convert.ToDateTime(logsFilter.EndDate).Date)
            .Where(x => EF.Functions.Like(x.SiteVisit.Site.Name, $"%{logsFilter.SiteName}%"))
            .Where(x => EF.Functions.Like(x.SiteVisit.User.Name, $"%{logsFilter.UserName}%"))
            .Where(x => EF.Functions.Like(x.SiteVisit.Site.Country, $"%{logsFilter.Country}%"))
            .Where(x => EF.Functions.Like(x.Command, $"%{logsFilter.Command}%"))
            .Where(x => EF.Functions.Like(x.Result, $"%{logsFilter.Result}%"))
            .OrderBy(x => x.SiteVisit.StartTime).ToListAsync();

            var logs = allLogs.GroupBy(x => x.SiteVisit.VisitId).Select(y => new
            {
                date = y.Select(i => i.SiteVisit.StartTime).Distinct(),
                country = y.Select(i => i.SiteVisit.Site.Country).Distinct(),
                site = y.Select(i => i.SiteVisit.Site.Name).Distinct(),
                longitude = y.Select(i => i.SiteVisit.Site.Longitude).Distinct(),
                latitude = y.Select(i => i.SiteVisit.Site.Latitude).Distinct(),
                rpi = y.Select(i => i.SiteVisit.RPIVersion).Distinct(),
                app = y.Select(i => i.SiteVisit.AppVersion).Distinct(),
                user = y.Select(i => i.SiteVisit.User.Name).Distinct(),
                command = y.Select(i => i.Command),
                result = y.Select(i => i.Result),
            });
            return new JsonResult(logs);
        }



    }
}
