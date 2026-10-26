
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;
using PoliceInvestigationMVC.Data;

public class PoliceOfficersController : Controller
{
    private readonly ApplicationDbContext _context;

    public PoliceOfficersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: POLICEOFFICERS
    public async Task<IActionResult> Index()    
    {
        var policeOfficers = await _context.PoliceOfficers
        .Include(p => p.Station)
        .ToListAsync();
        return View(policeOfficers);
    }

    // GET: POLICEOFFICERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policeofficer = await _context.PoliceOfficers
            .Include(p => p.Station)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (policeofficer == null)
        {
            return NotFound();
        }

        return View(policeofficer);
    }

    // GET: POLICEOFFICERS/Create
    public IActionResult Create()
    {
        ViewData["StationId"] = new SelectList(
        _context.PoliceStations,
        "Id",
        "Name");
        return View();
    }

    // POST: POLICEOFFICERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Rank,StationId,Station")] PoliceOfficer policeofficer)
    {
        if (ModelState.IsValid)
        {
            _context.Add(policeofficer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["StationId"] = new SelectList(
        _context.PoliceStations,
        "Id",
        "Name",
        policeofficer.StationId);

        return View(policeofficer);
    }

    // GET: POLICEOFFICERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policeofficer = await _context.PoliceOfficers.FindAsync(id);
        if (policeofficer == null)
        {
            return NotFound();
        }
        ViewData["StationId"] = new SelectList(
        _context.PoliceStations,
        "Id",
        "Name",
        policeofficer.StationId);

        return View(policeofficer);
    }

    // POST: POLICEOFFICERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id, 
        [Bind("Id,Name,Rank,StationId,Station")] PoliceOfficer policeofficer)
    {
        if (id != policeofficer.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(policeofficer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PoliceOfficerExists(policeofficer.Id))
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
        ViewData["StationId"] = new SelectList(
        _context.PoliceStations,
        "Id",
        "Name",
        policeofficer.StationId);

        return View(policeofficer);
    }

    // GET: POLICEOFFICERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policeofficer = await _context.PoliceOfficers
            .Include(p => p.Station)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (policeofficer == null)
        {
            return NotFound();
        }

        return View(policeofficer);
    }

    // POST: POLICEOFFICERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var policeofficer = await _context.PoliceOfficers.FindAsync(id);
        if (policeofficer != null)
        {
            _context.PoliceOfficers.Remove(policeofficer);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PoliceOfficerExists(int? id)
    {
        return _context.PoliceOfficers.Any(e => e.Id == id);
    }
}
