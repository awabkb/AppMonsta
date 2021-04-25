using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IMK_web.Models;
using IMK_web.Repository;
using IMK_web.Data;

namespace IMK_web.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardRepository _repo;

        public HomeController(ILogger<HomeController> logger, IDashboardRepository repo )
        {
            _logger = logger;
            this._repo = repo;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
