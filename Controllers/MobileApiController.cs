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
using System.Net.Mail;
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
        [HttpPost("user")]
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
                user.RegisteredAt = DateTime.Now;
                user.Email = userDto.Email == null ? User.Claims.Where(x => x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() : userDto.Email;

                if(!userDto.AspCompany.Equals("N/A"))
                    user.AspCompany = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
                else {
                    var asp = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
                    if(asp == null) {
                        AspCompany naAsp = new AspCompany();
                        Country country = await _appRepository.GetCountryByName(userDto.Country);
                        naAsp.Country = country;
                        naAsp.Name = userDto.AspCompany;
                        user.AspCompany = naAsp;
                    }
                    else 
                        user.AspCompany = asp;
                }

                _appRepository.Add(user);

                await _appRepository.SaveChanges();
                // await this.sendAccessRequest(userDto);
                UserToReturn userToReturnDto = new UserToReturn();
                userToReturnDto.AspCompany = user.AspCompany.Name;
                userToReturnDto.Email = user.Email;
                userToReturnDto.IsActive = user.IsActive;
                userToReturnDto.IsAdmin = user.IsAdmin;
                userToReturnDto.Name = user.Name;
                userToReturnDto.Phone = user.Phone;
                userToReturnDto.UserId = user.UserId;

                return Ok(userToReturnDto);
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

            Operator op = new Operator();
            var ops = await _appRepository.GetOperatorByCountry(siteVisitDto.Country);
            if (ops.Operators.Count()==0)
                op = null;
            else
            {
                if (ops.Operators.Count() > 1)
                {
                    var opname = this.GetOperatorBySite(siteVisitDto.SiteName, siteVisitDto.Country);
                    op = ops.Operators.FirstOrDefault(x => x.Name.Equals(opname));
                }
                else
                    op = ops.Operators.FirstOrDefault();
            }


            if (site == null)
            {
                site = new Site();
                site.Country = siteVisitDto.Country;
                site.Latitude = siteVisitDto.Latitude;
                site.Longitude = siteVisitDto.Longitude;
                site.Name = siteVisitDto.SiteName;
                site.Operator = op;

                _appRepository.Add(site);
            }

            if(site.Latitude == 0 && site.Longitude == 0){
                site.Latitude = siteVisitDto.Latitude;
                site.Longitude = siteVisitDto.Longitude;
                
                _appRepository.Update(site);
            }

            siteVisit.Site = site;
            siteVisit.StartTime = siteVisitDto.StartTime;
            siteVisit.FinishTime = siteVisitDto.FinishTime;
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
            siteVisit.AppVersion = siteVisitDto.AppVersion;
            siteVisit.RPIVersion = siteVisitDto.RpiVersion;
            //siteVisit.ImkVersion = await _appRepository.GetImkVersion(siteVisitDto.RpiVersion, siteVisitDto.AppVersion);

            Dictionary<string, int> imkFunctionsDic = new Dictionary<string, int>();

            foreach (Log log in logs)
            {

                var key = log.Command;

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
            iMK_Functions.FruStatus = imkFunctionsDic.GetValueOrDefault("frustatus");
            iMK_Functions.FruState = imkFunctionsDic.GetValueOrDefault("frustate");
            iMK_Functions.FruSerial = imkFunctionsDic.GetValueOrDefault("fruserial");
            iMK_Functions.FruProdNo = imkFunctionsDic.GetValueOrDefault("fruprodno");

            iMK_Functions.RetSerial = imkFunctionsDic.GetValueOrDefault("ret_serial");
            iMK_Functions.TMA = imkFunctionsDic.GetValueOrDefault("tma");
            iMK_Functions.RetAntenna = imkFunctionsDic.GetValueOrDefault("ret_antenna");

            iMK_Functions.VSWR = imkFunctionsDic.GetValueOrDefault("vswr");
            iMK_Functions.CPRI = imkFunctionsDic.GetValueOrDefault("cpri");

            iMK_Functions.Transport = imkFunctionsDic.GetValueOrDefault("transport");
            iMK_Functions.TransportRoutes = imkFunctionsDic.GetValueOrDefault("transport_routes");
            iMK_Functions.TransportInterfaces = imkFunctionsDic.GetValueOrDefault("transport_interfaces");

            iMK_Functions.MMEStatus = imkFunctionsDic.GetValueOrDefault("MME-status");
            iMK_Functions.GsmTRX = imkFunctionsDic.GetValueOrDefault("GSM-TRX");
            iMK_Functions.GsmState = imkFunctionsDic.GetValueOrDefault("GSM-State");
            iMK_Functions.SgwStatus = imkFunctionsDic.GetValueOrDefault("SGW-status");

            iMK_Functions.Traffic3g = imkFunctionsDic.GetValueOrDefault("traffic-3g");
            iMK_Functions.Traffic4g = imkFunctionsDic.GetValueOrDefault("traffic-4g");
            iMK_Functions.Traffic5g = imkFunctionsDic.GetValueOrDefault("traffic-5g");

            iMK_Functions.RSSIUMTS = imkFunctionsDic.GetValueOrDefault("rssi_umts");
            iMK_Functions.RSSIFDD = imkFunctionsDic.GetValueOrDefault("rssi-lte EUtranCellFDD");
            iMK_Functions.RSSITDD = imkFunctionsDic.GetValueOrDefault("rssi-lte EUtranCellTDD");
            iMK_Functions.RSSINR = imkFunctionsDic.GetValueOrDefault("rssi-nr");
            iMK_Functions.ExternalAlarm = imkFunctionsDic.GetValueOrDefault("external_alarm");
            iMK_Functions.Alarm = imkFunctionsDic.GetValueOrDefault("alarm");

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

            return Ok(countriesToReturn.OrderBy(o => o.Name).ToList());
        }


        ////////////////////////// Get User ////////////////////////////
        [HttpGet("User")]
        public async Task<IActionResult> getUser()
        {
            var userId = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            User user = await _appRepository.GetUser(userId);
            if (user != null)
                return Ok(user);
            else return BadRequest("User doesn't exist");
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
                user.AspCompany = await _appRepository.GetAspCompany(userDto.AspCompany);
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


        ////////////////////////// Send IMK User approval to admins ////////////////////////////

        [AllowAnonymous]
        [HttpPost("request")]
        public async Task<IActionResult> sendAccessRequest(UserDto userDto)
        {
            var aspCompany = await _appRepository.GetAspCompany(userDto.AspCompany);
            var aspManagers = await _appRepository.GetAspManagers(aspCompany.Country.Name);

            var url = "https://localhost:5001/api/mobileapi/activate";
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.office365.com";
            client.Port = 587;
            client.EnableSsl = true;
            System.Net.NetworkCredential ntcd = new System.Net.NetworkCredential("sara.shoujaa@lau.edu", "L@ucs1357ms");
            ntcd.UserName = "sara.shoujaa@lau.edu";
            ntcd.Password = "L@ucs1357ms";
            client.Credentials = ntcd;

            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("sara.shoujaa@lau.edu", "IMK Tool");
            foreach (var manager in aspManagers)
            {
                msg.To.Add(manager.Email);
            }

            msg.IsBodyHtml = true;
            string htmlString = @"<html>
                      <body>
                      <p>A new user started using IMK app</p>
                      <p> Email: " + userDto.Email +
                         "<br>Phone: " + userDto.Phone +
                         "<br>AspCompany: " + userDto.AspCompany + @"
                        </p>
                        <a href=" + url + "?email=" + userDto.Email + @">Click here to activate user</a>
                      </body>
                      </html>
                     ";


            msg.Body = htmlString;
            msg.Subject = "IMK Access - Req";
            client.SendAsync(msg, "msg");

            return Ok("sent");
        }

        [AllowAnonymous]
        [HttpGet("activate")]
        public async Task<IActionResult> activateUser(string email)
        {
            User user = await _appRepository.GetUserByEmail(email);

            if (user.IsActive == false)
            {
                user.IsActive = true;
                _appRepository.Update(user);
                await _appRepository.SaveChanges();
                return Ok("User is activated");
            }
            else
            {
                return Ok("user is already active");
            }
        }

        ////////////////////////// Get operator by site name rule for multioperator countries  ////////////////////////////
        public String GetOperatorBySite(string sitename, string country)
        {
            string op = "";
            if (country.Equals("Saudi Arabia"))
            {
                if (sitename.StartsWith("A") || sitename.StartsWith("E") || sitename.StartsWith("H") || sitename.StartsWith("T") || sitename.StartsWith("Z"))
                    op = "Saudi Telecom Company (STC)";
                else
                    op = "Mobily";
            }

            if (country.Equals("Bahrain"))
            {
                if (sitename.StartsWith("0") || sitename.StartsWith("1") || sitename.StartsWith("2") || sitename.StartsWith("3") || sitename.StartsWith("4") || sitename.StartsWith("5") || sitename.StartsWith("6") || sitename.StartsWith("7") || sitename.StartsWith("8") || sitename.StartsWith("9"))
                    op = "Zain";
                else
                    op = "Batelco";
            }

            if (country.Equals("Morocco"))
            {
                if (sitename.Contains("BB"))
                    op = "INWI";
                else
                    op = "MarocTel";
            }
            return op;
        }


    }
}
