
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

    // GET: WITNESSS
    public async Task<IActionResult> Index()    
    {
        var witnesses = await _context.Witnesses
        .Include(w => w.Investigation)
        .ToListAsync();

        return View(witnesses);
    }

    // GET: WITNESSS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (witness == null)
        {
            return NotFound();
        }

        return View(witness);
    }

    // GET: WITNESSS/Create
    public IActionResult Create()
    {
        ViewData["InvestigationId"] = new SelectList(
        _context.Investigations,
        "Id",
        "CaseNumber");

        return View();
    }

    // POST: WITNESSS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,ContactInformation,Statement,InvestigationId")] 
    Witness witness)
    {
        if (ModelState.IsValid)
        {
            _context.Add(witness);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        ViewData["InvestigationId"] = new SelectList(
        _context.Investigations,
        "Id",
        "CaseNumber",
        witness.InvestigationId);

        return View(witness);
    }

    // GET: WITNESSS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var witness = await _context.Witnesses.FindAsync(id);

        if (witness == null)
        {
            return NotFound();
        }
        ViewData["InvestigationId"] = new SelectList(
        _context.Investigations,
        "Id",
        "CaseNumber",
        witness.InvestigationId);

        return View(witness);
    }

    // POST: WITNESSS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int? id, 
    [Bind("Id,Name,ContactInformation,Statement,InvestigationId")] 
    Witness witness)
    {
        if (id != witness.Id)
        {
            return NotFound();
        }

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
        return View(witness);
    }

    // GET: WITNESSS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var witness = await _context.Witnesses
            .Include(w => w.Investigation)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (witness == null)
        {
            return NotFound();
        }

        return View(witness);
    }

    // POST: WITNESSS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var witness = await _context.Witnesses.FindAsync(id);
        if (witness != null)
        {
            _context.Witnesses.Remove(witness);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool WitnessExists(int? id)
    {
        return _context.Witnesses.Any(e => e.Id == id);
    }
}
