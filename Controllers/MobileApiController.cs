using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Data;
using IMK_web.Models;
using IMK_web.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace IMK_web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class MobileApiController : Controller
    {
        private readonly IAppRepository _appRepository;

        public MobileApiController(IAppRepository appRepository)
        {
            _appRepository = appRepository;
        }



        ////////////////////////// Create User ////////////////////////////
        [AllowAnonymous]
        [HttpPost("adduser")]
        public async Task<IActionResult> CreateUser(UserDto userDto)
        {
            var userId = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

            User user = await _appRepository.GetUser(userId);
            if (user == null)
            {
                user = new User();
                user.UserId = userId;
                user.Name = User.Claims.Where(x => x.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault() + " " + User.Claims.Where(x => x.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                user.Phone = userDto.Phone;
                user.Email = userDto.Email == null ? User.Claims.Where(x => x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() : userDto.Email;
                //user.AspCompany = userDto.AspCompany;
                _appRepository.Add(user);

                await _appRepository.SaveChanges();
                return Ok(user);
            }
            else
                return BadRequest("User already exists");

        }

        ////////////////////////// New Site Visit ////////////////////////////
        [HttpPost("sitevisit")]
        public async Task<IActionResult> CreateSiteVisit(SiteVisitDto siteVisitDto)
        {
            var userId = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            User user = await _appRepository.GetUser(userId);

            if (user == null)
            {
                return BadRequest("User not found " + userId);
            }

            SiteVisit siteVisit = new SiteVisit();

            siteVisit.User = await _appRepository.GetUser(userId);

            Site site = await _appRepository.GetSite(siteVisitDto.SiteName);
            if (site == null)
            {
                site = new Site();
                site.Country = siteVisitDto.Country;
                site.Latitude = siteVisitDto.Latitude.ToString();
                site.longitude = siteVisitDto.Longitude.ToString();

                _appRepository.Add(site);
            }

            siteVisit.Site = site;

            siteVisit.VistedAt = siteVisitDto.UploadedAt;
            ICollection<Log> logs = new List<Log>();
            foreach (LogDTO logDto in siteVisitDto.Actions)
            {
                logs.Add(new Log()
                {
                    LogId = logDto.LogId,
                    Command = logDto.Command,
                    Longitude = logDto.Longitude,
                    Latitude = logDto.Latitude,
                    Result = JsonConvert.SerializeObject(logDto.Result)
                });
            }
            siteVisit.Logs = logs;



            siteVisit.ImkVersion = await _appRepository.GetImkVersion(siteVisitDto.RpiVersion, siteVisitDto.AppVersion);

            Dictionary<string, int> imkFunctionsDic = new Dictionary<string, int>();

            foreach (Log log in logs)
            {

                string key = null;
                if (log.Command.StartsWith("fru"))
                {
                    key = "fru";
                }
                else if (log.Command.StartsWith("alarm"))
                {
                    key = "alarm";
                }
                else if (log.Command.StartsWith("rssi-lte"))
                {
                    key = "rssi-lte";
                }
                else
                {
                    key = log.Command;
                }
                if (imkFunctionsDic.ContainsKey(key))
                {
                    imkFunctionsDic[key]++;
                }
                else
                {
                    imkFunctionsDic.Add(key, 1);
                }
            }

            IMK_Functions iMK_Functions = new IMK_Functions();
            iMK_Functions.VSWR = imkFunctionsDic.GetValueOrDefault("vswr");
            iMK_Functions.FRU = imkFunctionsDic.GetValueOrDefault("fru");
            iMK_Functions.CPRI = imkFunctionsDic.GetValueOrDefault("cpri");
            iMK_Functions.IPROUT = imkFunctionsDic.GetValueOrDefault("transport_routes");
            iMK_Functions.IPInterfaces = imkFunctionsDic.GetValueOrDefault("transport_interfaces");
            iMK_Functions.Alarms = imkFunctionsDic.GetValueOrDefault("alarm");
            iMK_Functions.RetSerial = imkFunctionsDic.GetValueOrDefault("ret_serial");
            iMK_Functions.RETAntenna = imkFunctionsDic.GetValueOrDefault("ret_antenna");
            iMK_Functions.RSSIUMTS = imkFunctionsDic.GetValueOrDefault("rssi_umts");
            iMK_Functions.RSSILTE = imkFunctionsDic.GetValueOrDefault("rssi-lte");
            iMK_Functions.RSSINR = imkFunctionsDic.GetValueOrDefault("rssi-nr");
            siteVisit.IMK_Functions = iMK_Functions;

            _appRepository.Add(siteVisit);


            await _appRepository.SaveChanges();
            return Ok("Created");

        }


        ////////////////////////// Get Countries ////////////////////////////
        [AllowAnonymous]
        [HttpGet("countries")]
        public async Task<IActionResult> getCountries()
        {
            IEnumerable<Country> countries = await _appRepository.GetCountries();
            ICollection<CountryToReturn> countriesToReturn = new List<CountryToReturn>();
            foreach (Country country in countries)
            {
                CountryToReturn countryToReturn = new CountryToReturn()
                {
                    Code = country.Code,
                    Name = country.Name,
                    Operators = country.Operators.Select(x => x.Name).ToArray(),
                    AspCompanies = country.AspCompanies.Select(x => x.Name).ToArray()
                };
                countriesToReturn.Add(countryToReturn);
            }

            return Ok(countriesToReturn);
        }


        ////////////////////////// Get User ////////////////////////////
        [HttpGet("User")]
        public async Task<IActionResult> getUser(string userId)
        {
            User user = await _appRepository.GetUser(userId);
            return Ok(user);

        }

        ////////////////////////// Update User Info ////////////////////////////
        [HttpPut("user")]
        public async Task<IActionResult> updateUser(UserDto userDto)
        {

            var userId = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            User user = await _appRepository.GetUser(userId);
            if (user != null)
            {
                user.Name = User.Claims.Where(x => x.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault() + " " + User.Claims.Where(x => x.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                user.Phone = userDto.Phone;
                user.Email = userDto.Email;
                //user.AspCompany = userDto.AspCompany;
                _appRepository.Update(user);

                await _appRepository.SaveChanges();
                return Ok(user);
            }
            else
                return BadRequest("User has no profile");

        }


        ////////////////////////// Get Latest IMK Version ////////////////////////////
        [AllowAnonymous]
        [HttpGet("version")]
        public async Task<IActionResult> getLatestVersion()
        {

            var version = await _appRepository.GetLatestImkVersion();
            return Ok(version);
        }

    }
}
