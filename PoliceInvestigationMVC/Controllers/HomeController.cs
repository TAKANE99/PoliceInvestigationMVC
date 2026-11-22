using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using System.Diagnostics;
using PoliceInvestigationMVC.Exceptions;

namespace PoliceInvestigationMVC.Controllers
{
    // Manages the home page, investigation dashboard and privacy page.
    // Restricts access to authenticated users.
    [Authorize]
    public class HomeController : Controller
    {
        // Stores the logger and database context used by this controller.
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // Injects the logger and database context into the controller.
        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Directs users to the dashboard that matches their role.
        public IActionResult Index()
        {
            // Check whether the logged-in user is an Administrator.
            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            // Check whether the logged-in user is a Police Officer.
            if (User.IsInRole("PoliceOfficer"))
            {
                return RedirectToAction("Index", "OfficerDashboard");
            }

            // Prevent users without an assigned role from accessing a dashboard.
            return Forbid();
        }

        // Displays the application's privacy page.
        public IActionResult Privacy()
        {
            return View();
        }

        // Disables response caching and displays the error page with the request identifier.
        [ResponseCache(Duration = 0, 
            Location = ResponseCacheLocation.None, 
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id 
                ?? HttpContext.TraceIdentifier 
            });
        }

    }
}
