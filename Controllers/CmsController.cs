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

        [HttpPost("approver")]
        public async Task<ActionResult> addAspManager([FromQuery] string name, [FromQuery] string email, [FromQuery] string role, [FromQuery] string country)
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

        [HttpDelete("approver")]
        public async Task<ActionResult> removeApprover([FromQuery] string email)
        {
            AspManager approver = await _appRepository.GetApprover(email);
            _appRepository.Remove(approver);
            await _appRepository.SaveChanges();
            return Ok("Removed");
        }


        [HttpGet("approvers")]
        public async Task<ActionResult> GetApprovers()
        {
            var approvers = await _appRepository.GetApprovers();
            return Ok(approvers);
        }

        [HttpGet("asps")]
        public async Task<ActionResult> GetAspCompanies()
        {
            var companies = await _appRepository.GetAspCompanies();
            var asps = companies.GroupBy(x => new { x.Country.Name })
            .Select(y => new { country = y.Key.Name, asp = y.Select(i => i.Name) });
            return Ok(asps);
        }

        [HttpPost("asp")]
        public async Task<ActionResult> addAspCompany([FromQuery] string name, [FromQuery] string country)
        {

            AspCompany asp = new AspCompany();
            asp.Country = await _appRepository.GetCountryByName(country);
            asp.Name = name;
            _appRepository.Add(asp);
            await _appRepository.SaveChanges();

            return Ok(asp);
        }

        [HttpGet("users")]
        public async Task<ActionResult> getAllUsers([FromQuery] bool active)
        {
            var users = await _appRepository.GetAllUsers();

            var active_users = users.Where(x => x.IsActive == active && x.IsDeactivated==false).Select(x => new
            {
                name = x.Name,
                country = x.AspCompany.Country.Name,
                asp = x.AspCompany.Name,
                email = x.Email,
                phone = x.Phone,
                lastActive = x.SiteVisits.OrderByDescending(y => y.StartTime).Select(y => y.StartTime).FirstOrDefault(),
                registeredAt = x.RegisteredAt
            });
            return Ok(active_users);

        }

        [HttpGet("deactivated")]
        public async Task<ActionResult> getDeactivatedUsers()
        {
            var users = await _appRepository.GetAllUsers();

            var deactived_users = users.Where(x=>x.IsDeactivated == true).Select(x => new
            {
                name = x.Name,
                country = x.AspCompany.Country.Name,
                asp = x.AspCompany.Name,
                email = x.Email,
                phone = x.Phone,
                lastActive = x.SiteVisits.OrderByDescending(y => y.StartTime).Select(y => y.StartTime).FirstOrDefault(),
                registeredAt = x.RegisteredAt
            });
            return Ok(deactived_users);

        }

        [HttpPut("activate")]
        public async Task<ActionResult> ActivateUser([FromQuery] string email)
        {
            var user = await _appRepository.GetUserByEmail(email);
            user.IsActive = true;
            user.IsDeactivated = false;
            _appRepository.Update(user);
            await _appRepository.SaveChanges();
            return Ok(user);
        }


        [HttpPut("deactivate")]
        public async Task<ActionResult> DeactivateUser([FromQuery] string email)
        {
            var user = await _appRepository.GetUserByEmail(email);
            user.IsDeactivated = true;
            user.IsActive = false;
            _appRepository.Update(user);
            await _appRepository.SaveChanges();
            return Ok(user);
        }

        [HttpGet("logs")]
        public async Task<ActionResult> getLogs([FromQuery] string start, [FromQuery] string end)
        {
            var logs = await _appRepository.GetLogs(start, end);
            return Ok(logs);
        }


    }
}