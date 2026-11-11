
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

namespace PoliceInvestigationMVC.Controllers
{
    public class InvestigationOfficersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvestigationOfficersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all investigation team assignments
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.InvestigationOfficers
                .Include(io => io.Investigation)
                .Include(io => io.PoliceOfficer)
                .ToListAsync();

            return View(assignments);
        }

        // Display the assignment form
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // Save the new team assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestigationOfficer assignment)
        {
            bool investigationExists = await _context.Investigations
                .AnyAsync(i => i.Id == assignment.InvestigationId);

            bool officerExists = await _context.PoliceOfficers
                .AnyAsync(o => o.Id == assignment.PoliceOfficerId);

            if (!investigationExists || !officerExists)
            {
                ModelState.AddModelError("", "Please select a valid investigation and police officer.");
            }

            bool alreadyAssigned = await _context.InvestigationOfficers
                .AnyAsync(io =>
                    io.InvestigationId == assignment.InvestigationId &&
                    io.PoliceOfficerId == assignment.PoliceOfficerId);

            if (alreadyAssigned)
            {
                ModelState.AddModelError("", "This officer is already assigned to this investigation.");
            }

            if (ModelState.IsValid)
            {
                assignment.AssignedDate = DateTime.Now;

                _context.InvestigationOfficers.Add(assignment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns();
            return View(assignment);
        }

        // GET: Display the Edit Assignment page
        public async Task<IActionResult> Edit(int investigationId, int policeOfficerId)
        {
            var assignment = await _context.InvestigationOfficers
                .FirstOrDefaultAsync(io =>
                    io.InvestigationId == investigationId &&
                    io.PoliceOfficerId == policeOfficerId);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Save the updated team role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int investigationId,
            int policeOfficerId,
            string teamRole)
        {
            var assignment = await _context.InvestigationOfficers
                .FirstOrDefaultAsync(io =>
                    io.InvestigationId == investigationId &&
                    io.PoliceOfficerId == policeOfficerId);

            if (assignment == null)
            {
                return NotFound();
            }

            var allowedRoles = new[]
            {
        "Team Leader",
        "Lead Investigator",
        "Team Member",
        "Evidence Officer"
    };

            if (!allowedRoles.Contains(teamRole))
            {
                ModelState.AddModelError("teamRole", "Please select a valid team role.");
                return View(assignment);
            }

            assignment.TeamRole = teamRole;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Display the Delete confirmation page
        public async Task<IActionResult> Delete(int investigationId, int policeOfficerId)
        {
            var assignment = await _context.InvestigationOfficers
                .Include(io => io.Investigation)
                .Include(io => io.PoliceOfficer)
                .FirstOrDefaultAsync(io =>
                    io.InvestigationId == investigationId &&
                    io.PoliceOfficerId == policeOfficerId);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Remove the officer from the investigation team
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int investigationId, int policeOfficerId)
        {
            var assignment = await _context.InvestigationOfficers
                .FindAsync(investigationId, policeOfficerId);

            if (assignment != null)
            {
                _context.InvestigationOfficers.Remove(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Load investigations and officers for the dropdown lists
        private async Task LoadDropdowns()
        {
            ViewData["InvestigationId"] = new SelectList(
                await _context.Investigations.ToListAsync(),
                "Id",
                "CaseNumber");

            ViewData["PoliceOfficerId"] = new SelectList(
                await _context.PoliceOfficers.ToListAsync(),
                "Id",
                "Name");
        }
    }
}
