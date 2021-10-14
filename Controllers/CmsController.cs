using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Data;
using Data;
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
        private readonly IPortalRepository _portalRepository;


        public CmsController(IPortalRepository portalRepository)
        {
            _portalRepository = portalRepository;
        }

        [HttpPost("approver")]
        public async Task<ActionResult> addAspManager([FromQuery] string name, [FromQuery] string email, [FromQuery] string role, [FromQuery] string country)
        {
            AspManager manager = new AspManager();
            manager.Country = country;
            manager.Email = email;
            manager.Name = name;
            manager.Role = role;
            _portalRepository.Add(manager);
            await _portalRepository.SaveChanges();

            return Ok(manager);
        }

        [HttpDelete("approver")]
        public async Task<ActionResult> removeApprover([FromQuery] int id)
        {
            AspManager approver = await _portalRepository.GetApprover(id);
            _portalRepository.Remove(approver);
            await _portalRepository.SaveChanges();
            return Ok("Removed");
        }


        [HttpGet("approvers")]
        public async Task<ActionResult> GetApprovers()
        {
            var approvers = await _portalRepository.GetApprovers();
            return Ok(approvers);
        }

        [HttpGet("asps")]
        public async Task<ActionResult> GetAspCompanies()
        {
            var companies = await _portalRepository.GetAspCompanies();
            var asps = companies.GroupBy(x => new { x.Country.Name })
            .Select(y => new { country = y.Key.Name, asp = y.Select(i => i.Name) })
            .OrderBy(y => y.country);
            return Ok(asps);
        }

        [HttpPost("asp")]
        public async Task<ActionResult> addAspCompany([FromQuery] string name, [FromQuery] string country)
        {

            AspCompany asp = new AspCompany();
            asp.Country = await _portalRepository.GetCountryByName(country);
            asp.Name = name;
            _portalRepository.Add(asp);
            await _portalRepository.SaveChanges();

            return Ok(asp);
        }

        [HttpGet("users")]
        public async Task<ActionResult> getAllUsers([FromQuery] bool active)
        {
            var users = await _portalRepository.GetAllUsers();

            var active_users = users.Where(x => x.IsActive == active && x.IsDeactivated==false).Select(x => new
            {
                name = x.Name,
                country = x.AspCompany.Country.Name,
                asp = x.AspCompany.Name,
                email = x.Email,
                phone = x.Phone,
                lastActive = x.SiteVisits.OrderByDescending(y => y.StartTime).Select(y => y.StartTime).FirstOrDefault(),
                registeredAt = x.RegisteredAt
            }).OrderByDescending(y => y.lastActive);
            return Ok(active_users);

        }

        [HttpGet("deactivated")]
        public async Task<ActionResult> getDeactivatedUsers()
        {
            var users = await _portalRepository.GetAllUsers();

            var deactived_users = users.Where(x=>x.IsDeactivated == true).Select(x => new
            {
                name = x.Name,
                country = x.AspCompany.Country.Name,
                asp = x.AspCompany.Name,
                email = x.Email,
                phone = x.Phone,
                lastActive = x.SiteVisits.OrderByDescending(y => y.StartTime).Select(y => y.StartTime).FirstOrDefault(),
                registeredAt = x.RegisteredAt,
                status = x.Status
            }).OrderByDescending(y => y.lastActive);
            return Ok(deactived_users);

        }

        [HttpPut("activate")]
        public async Task<ActionResult> ActivateUser([FromQuery] string emails)
        {
            string [] allEmails = emails.Split(",");
            foreach(var email in allEmails)
            {
                var user = await _portalRepository.GetUserByEmail(email);
                user.IsActive = true;
                user.IsDeactivated = false;
                _portalRepository.Update(user);
            }
            await _portalRepository.SaveChanges();
            return Ok(allEmails);
        }


        [HttpPut("deactivate")]
        public async Task<ActionResult> DeactivateUser([FromQuery] string emails)
        {
            string [] allEmails = emails.Split(",");
            foreach(var email in allEmails)
            {
                var user = await _portalRepository.GetUserByEmail(email);
                user.IsDeactivated = true;
                user.IsActive = false;
                _portalRepository.Update(user);
            }
            await _portalRepository.SaveChanges();
            return Ok(allEmails);
        }

        [HttpGet("logs")]
        public async Task<ActionResult> getLogs([FromQuery] string start, [FromQuery] string end)
        {
            var logs = await _portalRepository.GetLogs(start, end);
            return Ok(logs);
        }
        [HttpGet("filtered-logs")]
        public async Task<ActionResult> FilterLogs([FromQuery] LogsFilter logsFilter)
        {
            var logs = await _portalRepository.GetFilteredLogs(logsFilter);
            return Ok(logs);
        }


    }
}