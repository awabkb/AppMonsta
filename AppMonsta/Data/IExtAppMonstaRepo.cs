using AppMonsta.Models;

namespace AppMonsta.Data
{
    public interface IExtAppMonstaRepo
    {
        Task<List<Genre>> GetGenresRanking(string store, string date, string countryCode);
        
        Task<List<AggregatedRanking>> GetAggregatedRankings(string store, string date, string countryCode) ;

        Task<AppDetails> GetAppDetails(string store,string countryCode, string appId);
        Task<List<AppDetails>> GetAllAppsDetails(string store, string date, string countryCode,string genreId);
    }
}
