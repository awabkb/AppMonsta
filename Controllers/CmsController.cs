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
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace IMK_web.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]

    [Route("dashboard/api/[controller]")]
    [ApiController]
    public class CmsController : Controller
    {
        private readonly IDashboardRepository _dashRepository;
        private readonly IAppRepository _appRepository;


        public CmsController(IDashboardRepository dashboardRepository, IAppRepository appRepository)
        {
            _dashRepository = dashboardRepository;
            _appRepository = appRepository;
        }

        [HttpPost("manager")]
        public async Task<ActionResult> addAspManager(string country, string email, string name, string role)
        {
            AspManager manager = new AspManager();
            manager.Country = country;
            manager.Email = email;
            manager.Name = name;
            manager.Role = role;
            _appRepository.Add(manager);
            await _appRepository.SaveChanges();

            return Ok(manager);
        }

        [HttpGet("approvers")]
        public async Task<ActionResult> GetApprovers()
        {
            var approvers = await _appRepository.GetApprovers();
            return Ok(approvers);

        }

        [HttpGet("users")]
        public async Task<ActionResult> getAllUsers([FromQuery] bool active)
        {
            var users = await _appRepository.GetAllUsers();
            if (active == true)
            {
                var active_users = users.Where(x => x.IsActive == true).Select(x => new
                {
                    name = x.Name,
                    country = x.AspCompany.Country.Name,
                    asp = x.AspCompany.Name,
                    email = x.Email,
                    phone = x.Phone,
                    registeredAt = x.RegisteredAt
                });
                return Ok(active_users);
            }
            else
            {
                var inactive_users = users.Where(x => x.IsActive == false).Select(x => new
                {
                    name = x.Name,
                    country = x.AspCompany.Country.Name,
                    asp = x.AspCompany.Name,
                    email = x.Email,
                    phone = x.Phone,
                    registeredAt = x.RegisteredAt
                });
                return Ok(inactive_users);
            }
        }

        [HttpPut("activate")]
        public async Task<ActionResult> ActivateUser([FromQuery] string email)
        {
            var user = await _appRepository.GetUserByEmail(email);
            user.IsActive = true;
            _appRepository.Update(user);
            await _appRepository.SaveChanges();
            return Ok(user);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult> DeactivateUser([FromQuery] string email)
        {
            var user = await _appRepository.GetUserByEmail(email);
            user.IsActive = false;
            _appRepository.Update(user);
            await _appRepository.SaveChanges();
            return Ok(user);
        }

        [HttpGet("logs")]
        public async Task<ActionResult> getLogs()
        {
            var allLogs = await _appRepository.GetLogs();
            var logs = allLogs.Select(x => new
            {
                id = x.SiteVisit.VisitId,
                date = x.SiteVisit.StartTime,
                country = x.SiteVisit.Site.Country,
                site = x.SiteVisit.Site.Name,
                longitude = x.Longitude,
                latitude = x.Latitude,
                rpi = x.SiteVisit.RPIVersion.ToString("0.00"),
                app = x.SiteVisit.AppVersion.ToString("0.00"),
                user = x.SiteVisit.User.Name,
                command = x.Command,
                result = x.Result
            }).ToList();
            return Ok(logs);
        }


    }
}