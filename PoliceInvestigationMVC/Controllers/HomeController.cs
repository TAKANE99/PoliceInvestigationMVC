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
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                int totalInvestigations = await _context.Investigations
                    .CountAsync();

                int openInvestigations = await _context.Investigations
                    .CountAsync(i => i.Status == "Open");

                int closedInvestigations = await _context.Investigations
                    .CountAsync(i => i.Status == "Closed");

                int highPriorityInvestigations = await _context.Investigations
                    .CountAsync(i => i.Priority == "High");

                var casesByStatus = await _context.Investigations
                    .GroupBy(i => i.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Total = g.Count()
                    })
                    .ToListAsync();

                ViewBag.TotalInvestigations = totalInvestigations;
                ViewBag.OpenInvestigations = openInvestigations;
                ViewBag.ClosedInvestigations = closedInvestigations;
                ViewBag.HighPriorityInvestigations = highPriorityInvestigations;
                ViewBag.CasesByStatus = casesByStatus;

                return View();

            }
            catch (InvestigationException e)
            {
                return View("Investigation not found " + e);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

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
