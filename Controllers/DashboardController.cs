using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Data;
using IMK_web.Models;
using IMK_web.Repository;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;


namespace IMK_web.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]

    public class DashboardController : Controller
    {
        public readonly IDashboardRepository _repo;
        public DashboardController(IDashboardRepository repo)
        {
            this._repo = repo;

        }
        public IActionResult Index(){

            ViewBag.Username = User.Claims.Where(c =>c.Type.Equals("name")).Select(c => c.Value).SingleOrDefault();

            return View();
        }
        

    }
}