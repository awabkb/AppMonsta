using System.Collections.Generic;
using System.Threading.Tasks;
using IMK_web.Data;
using IMK_web.Models;
using IMK_web.Repository;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
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

            return View();
        }

    }
}