using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrator,PoliceOfficer")]
public class WitnessesController : Controller
{
    // Provides access to the application's database.
    private readonly ApplicationDbContext _context;

    // Injects the database context into the controller.
    public WitnessesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Finds the police officer linked to the logged-in account.
    private async Task<PoliceOfficer?> GetLoggedInOfficerAsync()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _context.PoliceOfficers
            .FirstOrDefaultAsync(o => o.ApplicationUserId == userId);
    }

    // Loads investigations according to the logged-in user's role.
    private async Task PopulateInvestigationsAsync(int? selectedId = null)
    {
        if (User.IsInRole("Administrator"))
        {
            var allInvestigations = await _context.Investigations
                .ToListAsync();

            ViewData["InvestigationId"] = new SelectList(
                allInvestigations,
                "Id",
                "CaseNumber",
                selectedId);
        }
        else
        {
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                ViewData["InvestigationId"] = new SelectList(
                    Enumerable.Empty<Investigation>(),
                    "Id",
                    "CaseNumber");

                return;
            }

            var assignedInvestigations = await _context.Investigations
                .Where(i => i.InvestigatingOfficerId == officer.Id)
                .ToListAsync();

            ViewData["InvestigationId"] = new SelectList(
                assignedInvestigations,
                "Id",
                "CaseNumber",
                selectedId);
        }
    }

    // Displays all witnesses linked to a selected investigation.
    // GET: Witnesses/ByInvestigation/5
    public async Task<IActionResult> ByInvestigation(int investigationId)
    {
        var investigation = await _context.Investigations
            .FirstOrDefaultAsync(i => i.Id == investigationId);

        if (investigation == null)
        {
            return NotFound();
        }

        // Officers may only view witnesses for their assigned investigations.
        if (User.IsInRole("PoliceOfficer"))
        {
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            if (investigation.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }
        }

        // Load witnesses linked to the selected investigation.
        var witnesses = await _context.Witnesses
            .Include(w => w.Investigation)
            .Where(w => w.InvestigationId == investigationId)
            .ToListAsync();

        ViewBag.CaseNumber = investigation.CaseNumber;
        ViewBag.InvestigationId = investigationId;

        return View("ByInvestigation", witnesses);
    }

    // Retrieves and displays all registered witnesses together with their investigations.
    // GET: Witnesses
    public async Task<IActionResult> Index(string searchString)
    {
        var witnesses = _context.Witnesses
            .Include(w => w.Investigation)
            .AsQueryable();

        // Administrators can view all witnesses.
        if (User.IsInRole("Administrator"))
        {
            // No additional filtering required.
        }
        else
        {
            // Officers can only view witnesses linked to their assigned cases.
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            witnesses = witnesses.Where(w =>
                w.Investigation != null &&
                w.Investigation.InvestigatingOfficerId == officer.Id);
        }

        // Search by witness name, contact details or case number.
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            witnesses = witnesses.Where(w =>
                w.Name.Contains(searchString) ||
                w.ContactInformation.Contains(searchString) ||
                (w.Investigation != null &&
                 w.Investigation.CaseNumber.Contains(searchString)));
        }

        ViewData["CurrentFilter"] = searchString;

        return View(await witnesses.ToListAsync());
    }

    // Displays the details of a selected witness.
    // GET: Witnesses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (witness == null)
        {
            return NotFound();
        }

        // Officers may only view witnesses linked to their assigned cases.
        if (User.IsInRole("PoliceOfficer"))
        {
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            if (witness.Investigation?.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }
        }

        return View(witness);
    }

    // Displays the form for registering a new witness.
    // GET: Witnesses/Create
    [Authorize(Roles = "PoliceOfficer")]
    public async Task<IActionResult> Create()
    {
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Only show investigations assigned to the logged-in officer.
        await PopulateInvestigationsAsync();

        return View();
    }

    // Validates and saves a new witness record linked to an investigation. 
    // POST: Witnesses/Create
    [Authorize(Roles = "PoliceOfficer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("Name,ContactInformation,Statement,StatementDate,InvestigationId")]
    Witness witness)
    {
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Confirm that the selected investigation belongs to this officer.
        var assignedInvestigation = await _context.Investigations
            .FirstOrDefaultAsync(i =>
                i.Id == witness.InvestigationId &&
                i.InvestigatingOfficerId == officer.Id);

        if (assignedInvestigation == null)
        {
            return Forbid();
        }

        // Automatically set the statement date.
        witness.StatementDate = DateTime.Today;
        ModelState.Remove(nameof(Witness.StatementDate));

        if (ModelState.IsValid)
        {
            _context.Witnesses.Add(witness);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Witness registered successfully.";

            return RedirectToAction(nameof(Index));
        }

        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

    // Finds an existing witness and displays the edit form.
    // GET: Witnesses/Edit/5
    [Authorize(Roles = "PoliceOfficer")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (witness == null)
        {
            return NotFound();
        }

        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Confirm that the witness belongs to an investigation assigned to this officer.
        if (witness.Investigation?.InvestigatingOfficerId != officer.Id)
        {
            return Forbid();
        }

        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

    // Validates and updates an existing witness record.
    // POST: Witnesses/Edit/5
    [Authorize(Roles = "PoliceOfficer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Name,ContactInformation,Statement,StatementDate")]
    Witness witness)
    {
        if (id != witness.Id)
        {
            return NotFound();
        }

        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Retrieve the existing witness and its investigation.
        var existingWitness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (existingWitness == null)
        {
            return NotFound();
        }

        // Only allow editing witnesses linked to the officer's assigned cases.
        if (existingWitness.Investigation?.InvestigatingOfficerId != officer.Id)
        {
            return Forbid();
        }

        // Preserve the original investigation link.
        witness.InvestigationId = existingWitness.InvestigationId;
        ModelState.Remove(nameof(Witness.InvestigationId));

        if (ModelState.IsValid)
        {
            // Update only the permitted witness details.
            existingWitness.Name = witness.Name;
            existingWitness.ContactInformation = witness.ContactInformation;
            existingWitness.Statement = witness.Statement;
            existingWitness.StatementDate = witness.StatementDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Witness details updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        await PopulateInvestigationsAsync(existingWitness.InvestigationId);

        return View(witness);
    }

    // Witness deletion is disabled to preserve investigation records.
    // GET: Witnesses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (witness == null)
        {
            return NotFound();
        }

        TempData["ErrorMessage"] =
            "Deletion is not permitted. Witness records must be retained for investigation purposes.";

        return RedirectToAction(nameof(Index));
    }

    // Prevent deletion of witness records from the database.
    // POST: Witnesses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int? id)
    {
        return Forbid();
    }

    // Checks whether a witness record exists in the database.
    private bool WitnessExists(int id)
    {
        return _context.Witnesses.Any(w => w.Id == id);
    }
}
