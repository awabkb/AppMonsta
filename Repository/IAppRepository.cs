using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMK_web.Models;

namespace IMK_web.Repository
{
    public interface IAppRepository
    {

        void AddUser(User user);

        void AddSite(Site site);

        void AddOperator(Operator op);

        void AddAspCompany(AspCompany asp);

        void AddSiteVisit(SiteVisit visit);

        void AddLogs(Log logs);

        Task<Site> GetSite(string sitename);
        Task<User> GetUser(string userId);

        Task<ImkVersion> GetImkVersion(double rpi,double app);
        Task<bool> SaveChanges();

        Task<AspCompany> GetAspCompany(int aspId);
        Task<IEnumerable<Country>> GetCountries();

    }
}