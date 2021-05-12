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
        Task<Site> GetSite(string sitename);
        Task<User> GetUser(string userId);

        Task<ImkVersion> GetImkVersion(double rpi,double app);
        Task<bool> SaveChanges();

        Task<AspCompany> GetAspCompany(int aspId);
        Task<AspCompany> GetAspCompany(string aspName);
        Task<IEnumerable<Country>> GetCountries();
        Task<ImkVersion> GetLatestImkVersion();
        Task<AspManager> GetAspManager(int asp);
        Task<User> GetUserByEmail(string email);
        Task<IEnumerable<User>> GetAllUsers();
        Task<IEnumerable<Log>> GetLogs();




    }
}
