using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

public class WitnessesController : Controller
{
    // Provides access to the application's database.
    private readonly ApplicationDbContext _context;

    // Injects the database context into the controller.
    public WitnessesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Loads the Investigation dropdown and selects the relevant investigation when required.
    private async Task PopulateInvestigationsAsync(int? selectedId = null)
    {
        ViewData["InvestigationId"] = new SelectList(
            await _context.Investigations.ToListAsync(),
            "Id",
            "CaseNumber",
            selectedId);
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
    // Retrieves and displays witnesses, with an optional search filter.
    public async Task<IActionResult> Index(string searchString)
    {
        var witnesses = _context.Witnesses
            .Include(w => w.Investigation)
            .AsQueryable();

        // Search by witness name, contact details or investigation case number.
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
            return NotFound();

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (witness == null)
            return NotFound();

        return View(witness);
    }

    // Displays the form for registering a new witness.
    // GET: Witnesses/Create
    public async Task<IActionResult> Create()
    {
        await PopulateInvestigationsAsync();
        return View();
    }

    // Validates and saves a new witness record linked to an investigation. 
    // POST: Witnesses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,ContactInformation,Statement,StatementDate,InvestigationId")]
        Witness witness)
    {
        if (ModelState.IsValid)
        {
            _context.Witnesses.Add(witness);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Reloads the investigation dropdown if validation fails.
        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

    // Finds an existing witness and displays the edit form.
    // GET: Witnesses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var witness = await _context.Witnesses.FindAsync(id);

        if (witness == null)
            return NotFound();

        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

    // Validates and updates an existing witness record.
    // POST: Witnesses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,ContactInformation,Statement,StatementDate,InvestigationId")]
        Witness witness)
    {
        if (id != witness.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(witness);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WitnessExists(witness.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }
        // Reloads the investigation dropdown if the submitted information is invalid.
        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

    // Displays the selected witness before deletion.
    // GET: Witnesses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (witness == null)
            return NotFound();

        return View(witness);
    }

    // Removes the selected witness record from the database.
    // POST: Witnesses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var witness = await _context.Witnesses.FindAsync(id);

        if (witness != null)
        {
            _context.Witnesses.Remove(witness);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // Checks whether a witness record exists in the database.
    private bool WitnessExists(int id)
    {
        return _context.Witnesses.Any(w => w.Id == id);
    }
}
