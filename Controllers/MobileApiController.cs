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
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using SendGrid;
using SendGrid.Helpers.Mail;

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
            User tmp_user = await _appRepository.GetUserByEmail(userDto.Email == null ? User.Claims.Where(x => x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() : userDto.Email);

            User user = await _appRepository.GetUser(userId);

            if (user == null)
            {
                if (tmp_user != null) {
                    return BadRequest("Email already in use");
                }
                else
                {
                    user = new User();
                    user.UserId = userId;
                    user.Name = User.Claims.Where(x => x.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault() + " " + User.Claims.Where(x => x.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                    user.Phone = userDto.Phone;
                    user.RegisteredAt = DateTime.Now;
                    user.Email = userDto.Email == null ? User.Claims.Where(x => x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() : userDto.Email;

                    if (!userDto.AspCompany.Equals("N/A"))
                        user.AspCompany = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
                    else
                    {
                        var asp = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
                        if (asp == null)
                        {
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
                    //await this.sendAccessRequest(userDto).ConfigureAwait(false);
                    UserToReturn userToReturnDto = new UserToReturn();
                    userToReturnDto.AspCompany = user.AspCompany.Name;
                    userToReturnDto.Email = user.Email;
                    userToReturnDto.IsActive = user.IsActive;
                    userToReturnDto.IsAdmin = user.IsDeactivated;
                    userToReturnDto.Name = user.Name;
                    userToReturnDto.Phone = user.Phone;
                    userToReturnDto.UserId = user.UserId;
                    userToReturnDto.Message = "User has been created";

                    return Ok(new { Message = "User has been created" });
                }
            }
            else
            {
                if (!user.Email.Equals(userDto.Email) && tmp_user != null)
                {
                    return BadRequest("Email already in use");
                }
                else
                {
                    user.Name = User.Claims.Where(x => x.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault() + " " + User.Claims.Where(x => x.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                    user.Phone = userDto.Phone;
                    user.Email = userDto.Email;
                    if (!userDto.AspCompany.Equals("N/A"))
                        user.AspCompany = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
                    else
                    {
                        var asp = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
                        if (asp == null)
                        {
                            AspCompany naAsp = new AspCompany();
                            Country country = await _appRepository.GetCountryByName(userDto.Country);
                            naAsp.Country = country;
                            naAsp.Name = userDto.AspCompany;
                            user.AspCompany = naAsp;
                        }
                        else
                            user.AspCompany = asp;
                    }
                    _appRepository.Update(user);
                    await _appRepository.SaveChanges();
                    UserToReturn userToReturnDto = new UserToReturn();
                    userToReturnDto.AspCompany = user.AspCompany.Name;
                    userToReturnDto.Email = user.Email;
                    userToReturnDto.IsActive = user.IsActive;
                    userToReturnDto.IsAdmin = user.IsDeactivated;
                    userToReturnDto.Name = user.Name;
                    userToReturnDto.Phone = user.Phone;
                    userToReturnDto.UserId = user.UserId;
                    userToReturnDto.Message = "User has been updated";

                    return Ok(new { Message = "User has been updated" });
                }
            }

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
            if(siteVisitDto.SiteName == null || siteVisitDto.Country == null)
                return BadRequest("Upload Failed");
            
            if(siteVisitDto.CountryCode == null) {
                user.IsActive = false;
                user.IsDeactivated = true;
                user.Status = "System deactivation - old version";
                _appRepository.Update(user);
                await _appRepository.SaveChanges();
                return BadRequest("Your device is using and old version of IMK please update");
            }

            SiteVisit siteVisit = new SiteVisit();

            siteVisit.User = await _appRepository.GetUser(userId);

            Site site = await _appRepository.GetSite(siteVisitDto.SiteName, siteVisitDto.Country);

            Operator op = new Operator();
            var ops = await _appRepository.GetOperatorByCountry(siteVisitDto.CountryCode);
            if (ops == null)
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
                site.Country = _appRepository.GetCountry(siteVisitDto.CountryCode).Result.Name;
                site.Latitude = Convert.ToDouble(siteVisitDto.Latitude);
                site.Longitude = Convert.ToDouble(siteVisitDto.Longitude);
                site.Name = siteVisitDto.SiteName;
                site.Operator = op;

                _appRepository.Add(site);
            }

            if (site.Latitude == 0 && site.Longitude == 0)
            {
                site.Latitude = Convert.ToDouble(siteVisitDto.Latitude);
                site.Longitude = Convert.ToDouble(siteVisitDto.Longitude);

                _appRepository.Update(site);
            }

            siteVisit.Site = site;
            siteVisit.StartTime = siteVisitDto.StartTime;
            siteVisit.FinishTime = siteVisitDto.FinishTime;
            siteVisit.Brand = siteVisitDto.Brand;
            siteVisit.Model = siteVisitDto.Model;
            ICollection<Log> logs = new List<Log>();
            foreach (LogDTO logDto in siteVisitDto.Actions)
            {
                logs.Add(new Log()
                {
                    LogId = logDto.LogId,
                    Command = logDto.Command,
                    Longitude = logDto.Longitude,
                    Latitude = logDto.Latitude,
                    Result = JsonConvert.SerializeObject(logDto.Result),
                    ResponseTime = logDto.ResponseTime
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


            if (await _appRepository.SaveChanges())
                return Ok(siteVisitDto.Id);
            else
                return BadRequest("Upload Failed");

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
                user.AspCompany = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
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

        ////////////////////////// Add new Site Integration ////////////////////////////
        [HttpPost("integration")]
        public async Task<IActionResult> CreateSiteIntegration(SiteIntegration siteIntegration)
        {
            // if(siteIntegration.SiteName == null)
            //     return BadRequest("Invalid data");

            _appRepository.Add(new SiteIntegration()
            {
                SiteName = siteIntegration.SiteName,
                DownloadStart = siteIntegration.DownloadStart,
                DownloadEnd = siteIntegration.DownloadEnd,
                IntegrateStart = siteIntegration.IntegrateStart,
                IntegrateEnd = siteIntegration.IntegrateEnd,
                Outcome = siteIntegration.Outcome,
                Downloading = siteIntegration.Downloading,
                Integrating = siteIntegration.Integrating,                    
                UserId =  User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault(),
                MacAddress = siteIntegration.MacAddress,
                Longitude = siteIntegration.Longitude,
                Latitude = siteIntegration.Latitude,
                CountryCode = siteIntegration.CountryCode,
                CountryName = siteIntegration.CountryName,
                AppVersion = siteIntegration.AppVersion,
            });
            await _appRepository.SaveChanges();
            
            return Ok(siteIntegration);
            
        }

        ////////////////////////// Send IMK User approval to admins ////////////////////////////


        [AllowAnonymous]
        [HttpPost("request")]
        public async Task<IActionResult> sendAccessRequest(UserDto userDto)
        {
            var aspCompany = await _appRepository.GetAspCompanyByCountry(userDto.AspCompany, userDto.Country);
            var aspManagers = await _appRepository.GetAspManagers(aspCompany.Country.Name);

            var url = "https://localhost:5001/api/mobileapi/activate";

            string body = @"<html>
                      <body>
                      <p>A new user started using IMK app</p>
                      <p> Email: " + userDto.Email +
                         "<br>Phone: " + userDto.Phone +
                         "<br>Country: " + userDto.Country +
                         "<br>AspCompany: " + userDto.AspCompany + @"
                        </p>
                        <a href=" + url + "?accept=true&email=" + userDto.Email + @">Accept</a>
                        <a href=" + url + "?accept=false&email=" + userDto.Email + @">Reject</a>
                        <br><br>
                        <p>Please do not reply to this email</p>
                      </body>
                      </html>
                     ";


            await sendMail("No Reply - IMK Support", body, aspManagers, true);
            return Ok("sent");
        }

        [AllowAnonymous]
        [HttpGet("activate")]
        public async Task<IActionResult> activateUser(string email, bool accept)
        {
            User user = await _appRepository.GetUserByEmail(email);
            if (user != null)
            {
                if (accept == true)
                {
                    if (user.IsActive == false)
                    {
                        user.IsActive = true;
                        _appRepository.Update(user);
                        await _appRepository.SaveChanges();
                        return Ok(email + " is now activated");
                    }
                    else return Ok(email + " is already active");
                }
                else
                {
                    var visits = await _appRepository.GetUserSiteVisits(email);
                    if (visits.Count() == 0)
                    {
                        _appRepository.Remove(user);
                        await _appRepository.SaveChanges();
                        string[] recipients = new string[1];
                        recipients[0] = email;
                        var message = @"<html>
                                        <body>
                                        <p>Dear IMK User,</p>
                                        <p>Your account has been rejected by admin.</p>
                                            <br><br>
                                            <p>Please do not reply to this email</p>
                                        </body>
                                        </html>
                                        ";
                        await sendMail("No Reply - IMK Registration", message, recipients, false);
                        return Ok(email + "profile has been rejected");
                    }
                    else return Ok("Rejection Failed. User is already accepted");
                }
            }
            else return Ok("An account with this email is not found or had been removed.");
        }


        public async Task<IActionResult> sendMail(string Subject, string Body, string[] Recipients, bool BccAdmins)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("imk@ericsson.com", "IMK Support");
            var subject = Subject;
            var tos = new List<EmailAddress>();
            foreach (var recipient in Recipients)
            {
                tos.Add(new EmailAddress(recipient));   
            }
            var plainTextContent = "New message from IMK Support";
            var htmlContent = Body;
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            return Ok(response);
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

            if(country.Equals("Egypt"))
            {
                if(sitename.StartsWith("CAI") || sitename.StartsWith("DEL") || sitename.StartsWith("UCAI") || sitename.StartsWith("UDEL") || sitename.StartsWith("LCAI") || sitename.StartsWith("LDEL") || sitename.StartsWith("MCAI") || sitename.StartsWith("MDEL"))
                    op = "Etisalat";
                else
                    op = "Vodafone";
            }

            if(country.Equals("Oman"))
            {
                if(Regex.IsMatch(sitename, "^B[0-9]{2}") || sitename.StartsWith("EN") || sitename.StartsWith("GN"))
                    op = "Vodafone OM";
                else
                    op = "Omantel";
            }
            
            return op;

        }

        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<IActionResult> test()
        {   
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("imk@ericsson.com", "IMK Support");
            var subject = "IMK Email";
            var to = new EmailAddress("sara.shoujaa@ericsson.com", "User");
            var plainTextContent = "testing email feature";
            var htmlContent = "<strong>Test successful</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            return Ok(response);
        }


    }
}
