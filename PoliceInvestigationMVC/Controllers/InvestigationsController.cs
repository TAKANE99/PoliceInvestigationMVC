
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

public class InvestigationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InvestigationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: INVESTIGATIONS
    public async Task<IActionResult> Index()    
    {
        var investigations = await _context.Investigations
        .Include(i => i.InvestigatingOfficer)
        .ToListAsync();

        return View(investigations);
    }

    // GET: INVESTIGATIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var investigation = await _context.Investigations
            .Include(i => i.InvestigatingOfficer)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (investigation == null)
        {
            return NotFound();
        }

        return View(investigation);
    }

    // GET: INVESTIGATIONS/Create
    public IActionResult Create()
    {
        ViewData["InvestigatingOfficerId"] = new SelectList(
        _context.PoliceOfficers,
        "Id",
        "Name");

        return View(new Investigation { Status = "Open" });
    }

    // POST: INVESTIGATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([
        Bind("Id,CaseNumber,Title,Description,Status,CaseType,Priority,DateCreated,InvestigatingOfficerId,InvestigatingOfficer")]
    Investigation investigation)
    {
        if (ModelState.IsValid)
        {
            investigation.Status = "Open";

            _context.Add(investigation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        ViewData["InvestigatingOfficerId"] = new SelectList(
    _context.PoliceOfficers,
    "Id",
    "Name",
    investigation.InvestigatingOfficerId);

        return View(investigation);
    }

    // GET: INVESTIGATIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var investigation = await _context.Investigations.FindAsync(id);

        if (investigation == null)
        {
            return NotFound();
        }

        ViewData["InvestigatingOfficerId"] = new SelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            investigation.InvestigatingOfficerId);

        return View(investigation);
    }

    // POST: INVESTIGATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id, 
        [Bind("Id,CaseNumber,Title,Description,Status,CaseType,Priority,DateCreated,InvestigatingOfficerId")] 
    Investigation investigation)
    {
        if (id != investigation.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(investigation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvestigationExists(investigation.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["InvestigatingOfficerId"] = new SelectList(
        _context.PoliceOfficers,
        "Id",
        "Name",
        investigation.InvestigatingOfficerId);

        return View(investigation);
    }

    // GET: INVESTIGATIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var investigation = await _context.Investigations
            .Include(i => i.InvestigatingOfficer)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (investigation == null)
        {
            return NotFound();
        }

        return View(investigation);
    }

    // POST: INVESTIGATIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var investigation = await _context.Investigations.FindAsync(id);
        if (investigation != null)
        {
            _context.Investigations.Remove(investigation);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool InvestigationExists(int? id)
    {
        return _context.Investigations.Any(e => e.Id == id);
    }
}
