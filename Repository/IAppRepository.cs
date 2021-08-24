using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMK_web.Models;

namespace IMK_web.Repository
{
    public interface IAppRepository
    {

	    void Add<T>(T entity) where T: class;
        void Update<T>(T entity) where T: class;
        void Remove<T>(T entity) where T: class;
        Task<Site> GetSite(string sitename, string country);
        Task<User> GetUser(string userId);
        Task<Country> GetCountryByName(string country);
        Task<Country> GetCountry(string code);
        Task<ImkVersion> GetImkVersion(double rpi,double app);
        Task<bool> SaveChanges();
        Task<AspCompany> GetAspCompanyByCountry(string aspName, string country);
        Task<AspCompany> GetAspCompany(string aspName);
        Task<Country> GetOperatorByCountry(string country);
        Task<IEnumerable<Country>> GetCountries();
        Task<ImkVersion> GetLatestImkVersion();
        Task<string []> GetAspManagers(string country);
        Task<string []> GetAdmins();
        Task<IEnumerable<SiteVisit>> GetUserSiteVisits(User user);
        Task<User> GetUserByEmail(string email);
        Task<IEnumerable<User>> GetAllUsers();
        Task<ActionResult> GetLogs(string start, string end);
        Task<IEnumerable<AspManager>> GetApprovers();
        Task<AspManager> GetApprover(string email);
        Task<IEnumerable<AspCompany>> GetAspCompanies();





    }
}
