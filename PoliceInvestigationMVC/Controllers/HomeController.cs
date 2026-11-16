using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
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

        // Retrieves investigation statistics and displays them on the dashboard.
        public async Task<IActionResult> Index()
        {
            try
            {
                // Retrieves investigation statistics and displays them on the dashboard.
                int totalInvestigations = await _context.Investigations
                    .CountAsync();

                int openInvestigations = await _context.Investigations
                    .CountAsync(i => i.Status == "Open");

                int closedInvestigations = await _context.Investigations
                    .CountAsync(i => i.Status == "Closed");

                int highPriorityInvestigations = await _context.Investigations
                    .CountAsync(i => i.Priority == "High");

                // Groups investigations by status and calculates the total in each group.
                var casesByStatus = await _context.Investigations
                    .GroupBy(i => i.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Total = g.Count()
                    })
                    .ToListAsync();

                // Groups investigations by priority and calculates the total in each group.
                var casesByPriority = await _context.Investigations
                    .GroupBy(i => i.Priority)
                    .Select(g => new
                    {
                        Priority = g.Key,
                        Total = g.Count()
                    })
                    .ToListAsync();
                // Sends the investigation statistics to the dashboard view.
                ViewBag.TotalInvestigations = totalInvestigations;
                ViewBag.OpenInvestigations = openInvestigations;
                ViewBag.ClosedInvestigations = closedInvestigations;
                ViewBag.HighPriorityInvestigations = highPriorityInvestigations;
                ViewBag.CasesByStatus = casesByStatus;
                ViewBag.CasesByPriority = casesByPriority;

                return View();

            }
            // Handles the custom investigation exception.
            catch (InvestigationException e)
            {
                return View("Investigation not found " + e);
            }
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
