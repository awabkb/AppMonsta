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

namespace IMK_web.Controllers
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
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
            var userId = User.Claims.Where(x =>x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

            User user = await _appRepository.GetUser(userId);
            if (user==null)
            {
                user= new User();
                user.UserId = userId;
                user.Name =  User.Claims.Where(x =>x.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault() +" "+ User.Claims.Where(x =>x.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                //user.Phone = userDto.Phone;
                user.Email = userDto.Email==null?User.Claims.Where(x =>x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault():userDto.Email;
                //user.AspCompany = userDto.AspCompany;
                _appRepository.AddUser(user);

                await _appRepository.SaveChanges();
                return Ok(user);
            }
            else
                return BadRequest("User already exists");
            
        }

        //[AllowAnonymous]
        [HttpPost("sitevisit")]
        public async Task<IActionResult> CreateSiteVisit(SiteVisitDto siteVisitDto)
        {
            var userId = User.Claims.Where(x =>x.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            User user = await _appRepository.GetUser(userId);
            
            if(user==null){
                return BadRequest();
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
                
                _appRepository.AddSite(site);
            }
            
            siteVisit.Site = site;

            siteVisit.VistedAt = siteVisitDto.UploadedAt;
            IEnumerable<Log> logs = siteVisitDto.Actions;
            siteVisit.Logs = logs;
            


            siteVisit.ImkVersion = await _appRepository.GetImkVersion(siteVisitDto.RpiVersion,siteVisitDto.AppVersion);

            Dictionary<string,int> imkFunctionsDic = new Dictionary<string, int>();

            foreach(Log log in logs){
                
                string key = null;
                if(log.Command.StartsWith("fru")){
                     key= "fru";
                }
                else if(log.Command.StartsWith("alarm")){
                    key = "alarm";
                }
                else if(log.Command.StartsWith("rssi-lte")){
                     key= "rssi-lte";
                }
                else{
                     key=log.Command;
                }
               if(imkFunctionsDic.ContainsKey(key)){
                   imkFunctionsDic[key]++;
               }
               else{
                   imkFunctionsDic.Add(key,1);
               }
            }
            
            IMK_Functions iMK_Functions= new IMK_Functions();
            iMK_Functions.VSWR = imkFunctionsDic["vswr"];
            iMK_Functions.FRU = imkFunctionsDic["fru"];
            iMK_Functions.CPRI = imkFunctionsDic["cpri"];
            iMK_Functions.IPROUT = imkFunctionsDic["transport_routes"];
            iMK_Functions.IPInterfaces = imkFunctionsDic["transport_interfaces"];
            iMK_Functions.Alarms = imkFunctionsDic["alarm"];
            iMK_Functions.RetSerial = imkFunctionsDic["ret_serial"];
            iMK_Functions.RETAntenna = imkFunctionsDic["ret_antenna"];
            iMK_Functions.RSSIUMTS = imkFunctionsDic["rssi_umts"];
            iMK_Functions.RSSILTE = imkFunctionsDic["rssi-lte"];
            iMK_Functions.RSSINR = imkFunctionsDic["rssi-nr"];
            
            siteVisit.IMK_Functions = iMK_Functions;
            
            _appRepository.AddSiteVisit(siteVisit);


            await _appRepository.SaveChanges();
            return Ok("Created");

        }
        [AllowAnonymous]
        [HttpGet("countries")]
        public async Task<IActionResult> getCountries()
        {
            var countries = await _appRepository.GetCountries();
            return Ok(countries);
        }

        // [AllowAnonymous]
        // [HttpPost("user")]
        // public async Task<IActionResult> createUser(UserDto userToCreate){
        //     User user =await _appRepository.GetUser(userToCreate.UserId);

        //    if( user==null){
        //        user = new User();
        //         user.Email = userToCreate.Email;
        //         user.UserId = userToCreate.UserId;
        //    }
        //     return Ok(user);

        // }

        [HttpGet("User")]
        public async Task<IActionResult> getUser(string userId){
            User user =await _appRepository.GetUser(userId);
            return Ok(user);

        }


        //////////////////////////// Create Site /////////////////////////////////

        //////////////////////////// Create Operator /////////////////////////////

        //////////////////////////// Create Company //////////////////////////////

        //////////////////////////// Create SiteVisit ////////////////////////////

        //////////////////////////// Create Logs /////////////////////////////////

    }
}