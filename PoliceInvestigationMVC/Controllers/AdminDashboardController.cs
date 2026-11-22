using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;

namespace PoliceInvestigationMVC.Controllers
{
    // Only users with the Administrator role can access this controller.
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Connects the controller to the application database.
        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays the Administrator Dashboard and system statistics.
        public async Task<IActionResult> Index()
        {
            // Total investigations registered in the system.
            ViewBag.TotalCases = await _context.Investigations.CountAsync();

            // Active investigations include Open and In Progress cases.
            ViewBag.OpenCases = await _context.Investigations
                .CountAsync(i => i.Status == "Open" || i.Status == "In Progress");

            // Closed investigations.
            ViewBag.ClosedCases = await _context.Investigations
                .CountAsync(i => i.Status == "Closed");

            // Investigations without a main investigating officer.
            ViewBag.UnassignedCases = await _context.Investigations
                .CountAsync(i => i.InvestigatingOfficerId == null);

            // Total police officers and stations.
            ViewBag.TotalOfficers = await _context.PoliceOfficers.CountAsync();
            ViewBag.TotalStations = await _context.PoliceStations.CountAsync();
            // Retrieve investigations that still need a main investigating officer.
            ViewBag.UnassignedInvestigations = await _context.Investigations
                .Include(i => i.InvestigatingOfficer)
                .Where(i => i.InvestigatingOfficerId == null)
                .OrderByDescending(i => i.DateCreated)
                .ToListAsync();

            return View();
        }
    }
}