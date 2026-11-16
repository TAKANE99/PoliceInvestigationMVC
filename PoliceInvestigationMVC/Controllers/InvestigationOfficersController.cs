
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

namespace PoliceInvestigationMVC.Controllers
{
    // Manages police officer assignments to investigation teams.
    public class InvestigationOfficersController : Controller
    {
        // Provides access to the application's database.
        private readonly ApplicationDbContext _context;

        // Injects the database context into the controller.
        public InvestigationOfficersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves and displays all investigation team assignments with their related investigations and officers.
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.InvestigationOfficers
                .Include(io => io.Investigation)
                .Include(io => io.PoliceOfficer)
                .ToListAsync();

            return View(assignments);
        }

        // Displays the form for assigning a police officer to an investigation.
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // Validates and saves a new police officer assignment to an investigation team.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestigationOfficer assignment)
        {
            // Checks whether the selected investigation and police officer exist.
            bool investigationExists = await _context.Investigations
                .AnyAsync(i => i.Id == assignment.InvestigationId);
            bool officerExists = await _context.PoliceOfficers
                .AnyAsync(o => o.Id == assignment.PoliceOfficerId);

            if (!investigationExists || !officerExists)
            {
                ModelState.AddModelError("", "Please select a valid investigation and police officer.");
            }

            // Prevents the same police officer from being assigned to the same investigation more than once.
            bool alreadyAssigned = await _context.InvestigationOfficers
                .AnyAsync(io =>
                    io.InvestigationId == assignment.InvestigationId &&
                    io.PoliceOfficerId == assignment.PoliceOfficerId);

            if (alreadyAssigned)
            {
                ModelState.AddModelError("", "This officer is already assigned to this investigation.");
            }

            // Saves the assignment if all validation checks pass.
            if (ModelState.IsValid)
            {
                assignment.AssignedDate = DateTime.Now;

                _context.InvestigationOfficers.Add(assignment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reloads dropdown lists if validation fails.
            await LoadDropdowns();
            return View(assignment);
        }

        // Retrieves an existing team assignment and displays the edit page.
        // GET: Display the Edit Assignment page
        public async Task<IActionResult> Edit(int investigationId, int policeOfficerId)
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

        // Validates and updates the team role of an assigned police officer.
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

        // Displays an investigation team assignment before it is deleted.
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

        // Removes a police officer from the selected investigation team.
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

        // Loads investigations and police officers into their respective dropdown lists.
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
