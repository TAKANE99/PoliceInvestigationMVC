using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using System.Security.Claims;

namespace PoliceInvestigationMVC.Controllers
{
    // Only Police Officers can record investigation progress.
    [Authorize(Roles = "Administrator,PoliceOfficer")]
    public class InvestigationProgressController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvestigationProgressController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays the progress recording form.
        [Authorize(Roles = "PoliceOfficer")]
        public async Task<IActionResult> Create(int? investigationId)
        {
            if (investigationId == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var officer = await _context.PoliceOfficers
                .FirstOrDefaultAsync(o => o.ApplicationUserId == userId);

            if (officer == null)
            {
                return Forbid();
            }

            // Confirm that this investigation belongs to the logged-in officer.
            var investigation = await _context.Investigations
                .FirstOrDefaultAsync(i =>
                    i.Id == investigationId &&
                    i.InvestigatingOfficerId == officer.Id);

            if (investigation == null)
            {
                return NotFound();
            }

            ViewBag.CaseNumber = investigation.CaseNumber;
            ViewBag.InvestigationTitle = investigation.Title;

            return View(new InvestigationProgress
            {
                InvestigationId = investigation.Id,
                ActivityDate = DateTime.Today
            });
        }

        // Saves the progress update.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PoliceOfficer")]
        public async Task<IActionResult> Create(
            InvestigationProgress progress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var officer = await _context.PoliceOfficers
                .FirstOrDefaultAsync(o => o.ApplicationUserId == userId);

            if (officer == null)
            {
                return Forbid();
            }

            // Confirm that the officer is assigned to this investigation.
            var investigation = await _context.Investigations
                .FirstOrDefaultAsync(i =>
                    i.Id == progress.InvestigationId &&
                    i.InvestigatingOfficerId == officer.Id);

            if (investigation == null)
            {
                return NotFound();
            }

            // Generate the activity date and recording date automatically.
            progress.ActivityDate = DateTime.Now;
            ModelState.Remove(nameof(InvestigationProgress.ActivityDate));

            if (ModelState.IsValid)
            {
                // Set the officer and recording date on the server.
                progress.PoliceOfficerId = officer.Id;
                progress.DateRecorded = DateTime.Now;

                _context.InvestigationProgressRecords.Add(progress);

                // Update the investigation status when work begins.
                if (investigation.Status == "Open")
                {
                    investigation.Status = "In Progress";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Investigation progress recorded successfully.";

                return RedirectToAction(
                    "Index", "OfficerDashboard");
            }

            ViewBag.CaseNumber = investigation.CaseNumber;
            ViewBag.InvestigationTitle = investigation.Title;

            return View(progress);
        }


        // Displays progress history for an investigation.
        [Authorize(Roles = "Administrator,PoliceOfficer")]
        public async Task<IActionResult> History(int? investigationId)
        {
            if (investigationId == null)
            {
                return NotFound();
            }

            var investigation = await _context.Investigations
                .FirstOrDefaultAsync(i => i.Id == investigationId);

            if (investigation == null)
            {
                return NotFound();
            }

            // Administrators can review any investigation.
            if (!User.IsInRole("Administrator"))
            {
                // Identify the logged-in police officer.
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var officer = await _context.PoliceOfficers
                    .FirstOrDefaultAsync(o => o.ApplicationUserId == userId);

                if (officer == null)
                {
                    return Forbid();
                }

                bool isAssigned =
                    investigation.InvestigatingOfficerId == officer.Id ||
                    await _context.InvestigationOfficers.AnyAsync(io =>
                        io.InvestigationId == investigation.Id &&
                        io.PoliceOfficerId == officer.Id);

                if (!isAssigned)
                {
                    return Forbid();
                }
            }

            // Retrieve all progress updates for this investigation.
            var progressUpdates = await _context.InvestigationProgressRecords
                .Include(p => p.PoliceOfficer)
                .Where(p => p.InvestigationId == investigation.Id)
                .OrderByDescending(p => p.ActivityDate)
                .ToListAsync();

            ViewBag.CaseNumber = investigation.CaseNumber;
            ViewBag.InvestigationTitle = investigation.Title;
            ViewBag.InvestigationId = investigation.Id;

            return View(progressUpdates);
        }
    }
}
