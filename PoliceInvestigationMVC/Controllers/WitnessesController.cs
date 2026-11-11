using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

public class WitnessesController : Controller
{
    private readonly ApplicationDbContext _context;

    public WitnessesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Load the Investigation dropdown.
    private async Task PopulateInvestigationsAsync(int? selectedId = null)
    {
        ViewData["InvestigationId"] = new SelectList(
            await _context.Investigations.ToListAsync(),
            "Id",
            "CaseNumber",
            selectedId);
    }

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

        return View("ByInvestigation", witnesses);
    }

    // GET: Witnesses
    public async Task<IActionResult> Index()
    {
        var witnesses = await _context.Witnesses
            .Include(w => w.Investigation)
            .ToListAsync();

        return View(witnesses);
    }

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

    // GET: Witnesses/Create
    public async Task<IActionResult> Create()
    {
        await PopulateInvestigationsAsync();
        return View();
    }

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

        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

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

        await PopulateInvestigationsAsync(witness.InvestigationId);

        return View(witness);
    }

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

    private bool WitnessExists(int id)
    {
        return _context.Witnesses.Any(w => w.Id == id);
    }
}
