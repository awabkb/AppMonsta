using AppMonsta.Data;
using AppMonsta.Dtos;
using AppMonsta.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AppMonsta.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class HomeController : Controller
    {
        private readonly IExtAppMonstaRepo extAppMonstaRepo;

        public HomeController(IExtAppMonstaRepo extAppMonstaRepo)

        {
            this.extAppMonstaRepo = extAppMonstaRepo;

        }

        [HttpGet("GetGenresRanking")]
        public async Task<IActionResult> GetGenresRanking([FromBody] GenreRankingRequest genreRankingRequest)
        {
            List<Genre> genres = await extAppMonstaRepo.GetGenresRanking(genreRankingRequest.Store, genreRankingRequest.Date, genreRankingRequest.CountryCode);
            return Ok(genres);
        }

        [HttpPost("GetAggregatedRankings")]
        public async Task<IActionResult> GetAggregatedRankings([FromBody] GenreRankingRequest genreRankingRequest)
        {
            try
            {
                List<AggregatedRanking> aggregatedRankings = await extAppMonstaRepo.GetAggregatedRankings(genreRankingRequest.Store, genreRankingRequest.Date, genreRankingRequest.CountryCode);


                return Ok(aggregatedRankings);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
     
        }
        [HttpPost("GetAppDetails")]
        public async Task<IActionResult> GetAppDetails([FromBody] AppDetailsRequest appDetailsRequest)
        {
            try
            {
                AppDetails appDetails = await extAppMonstaRepo.GetAppDetails(appDetailsRequest.Store, appDetailsRequest.CountryCode, appDetailsRequest.AppId);
                return Ok(appDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
        [HttpPost("GetAllAppsDetails")]
        public async Task<IActionResult> GetAllAppsDetails([FromBody] AllAppsDetailsRequest appsDetailsRequest)
        {
            try
            {
                List<AppDetails> appsDetails = await extAppMonstaRepo.GetAllAppsDetails(appsDetailsRequest.Store, appsDetailsRequest.Date, appsDetailsRequest.CountryCode, appsDetailsRequest.GenreId);
                return Ok(appsDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
