
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;
using PoliceInvestigationMVC.Data;

public class PoliceStationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PoliceStationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: POLICESTATIONS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.PoliceStations.ToListAsync());
    }

    // GET: POLICESTATIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policestation = await _context.PoliceStations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (policestation == null)
        {
            return NotFound();
        }

        return View(policestation);
    }

    // GET: POLICESTATIONS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: POLICESTATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Location")] PoliceStation policestation)
    {
        if (ModelState.IsValid)
        {
            _context.Add(policestation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(policestation);
    }

    // GET: POLICESTATIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policestation = await _context.PoliceStations.FindAsync(id);
        if (policestation == null)
        {
            return NotFound();
        }
        return View(policestation);
    }

    // POST: POLICESTATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Location")] PoliceStation policestation)
    {
        if (id != policestation.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(policestation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PoliceStationExists(policestation.Id))
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
        return View(policestation);
    }

    // GET: POLICESTATIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policestation = await _context.PoliceStations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (policestation == null)
        {
            return NotFound();
        }

        return View(policestation);
    }

    // POST: POLICESTATIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var policestation = await _context.PoliceStations.FindAsync(id);
        if (policestation != null)
        {
            _context.PoliceStations.Remove(policestation);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PoliceStationExists(int? id)
    {
        return _context.PoliceStations.Any(e => e.Id == id);
    }
}
