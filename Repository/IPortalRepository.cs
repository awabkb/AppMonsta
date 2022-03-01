using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IMK_web.Models;
using Data;

namespace IMK_web.Repository
{
    public interface IPortalRepository
    {

        void Add<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        void Remove<T>(T entity) where T : class;
        Task<bool> SaveChanges();
        Task<IEnumerable<Country>> GetCountries();
        Task<ActionResult> GetLogs(string start, string end);
        Task<ActionResult> GetFilteredLogs(LogsFilter logsFilter);
        Task<IEnumerable<AspManager>> GetApprovers();
        Task<AspManager> GetApprover(int id);
        Task<IEnumerable<AspCompany>> GetAspCompanies();
        Task<Country> GetCountryByName(string country);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserByEmail(string email);
        Task<IEnumerable<Rating>> GetRatings();
        Task<IEnumerable<RatingQuestion>> GetRatingQuestions();
        Task<RatingQuestion> GetRatingQuestion(int id);
        AspCompany GetSingleAspCompany(string companyName, string countryName);

    }
}
