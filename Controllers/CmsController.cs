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

    [Route("api/[controller]")]
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

        [HttpPost("addManager")]
        public async Task<ActionResult> addAspManager(string asp, string email)
        {
            var aspCompany = await _appRepository.GetAspCompany(asp);

            AspManager manager = new AspManager();
            manager.AspCompany = aspCompany;
            manager.Email = email;
            _appRepository.Add(manager);
            await _appRepository.SaveChanges();

            return Ok(manager);
        }


        [HttpGet("allUsers")]
        public async Task<ActionResult> getAllUsers()
        {
            var users = await _appRepository.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("logs")]
        public async Task<ActionResult> getLogs()
        {
            var logs = await _appRepository.GetLogs();
            return Ok(logs);
        }


    }
}