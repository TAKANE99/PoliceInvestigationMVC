using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using System.Security.Claims;

namespace PoliceInvestigationMVC.Controllers
{
    // Only logged-in Police Officers can access this dashboard.
    [Authorize(Roles = "PoliceOfficer")]
    public class OfficerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfficerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays only the investigations assigned to the logged-in officer.
        public async Task<IActionResult> Index()
        {
            // Get the ID of the currently logged-in account.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the officer profile linked to that account.
            var officer = await _context.PoliceOfficers
                .Include(o => o.Station)
                .FirstOrDefaultAsync(o => o.ApplicationUserId == userId);

            // Stop if the account is not linked to an officer profile.
            if (officer == null)
            {
                return Forbid();
            }

            // Retrieve only investigations assigned to this officer.
            var investigations = await _context.Investigations
                .Where(i => i.InvestigatingOfficerId == officer.Id)
                .OrderByDescending(i => i.DateCreated)
                .ToListAsync();

            ViewBag.OfficerName = officer.Name;
            ViewBag.ServiceNumber = officer.ServiceNumber;
            ViewBag.Rank = officer.Rank;
            ViewBag.StationName = officer.Station?.Name;
            ViewBag.ContactNumber = officer.ContactNumber;
            ViewBag.Email = officer.Email;

            ViewBag.TotalCases = investigations.Count;
            ViewBag.OpenCases = investigations.Count(i => i.Status == "Open");
            ViewBag.ClosedCases = investigations.Count(i => i.Status == "Closed");

            return View(investigations);
        }
    }
}