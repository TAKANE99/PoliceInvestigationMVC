
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using static System.Collections.Specialized.BitVector32;

// Manages police station operations, including creating, viewing,
// editing and deleting police station records.
public class PoliceStationsController : Controller
{
    // Stores the database context to provide access to police station records.
    private readonly ApplicationDbContext _context;

    // Receives the database context through dependency injection.
    public PoliceStationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Receives the database context through dependency injection.
    // GET: POLICESTATIONS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.PoliceStations.ToListAsync());
    }

    // Retrieves and displays the details of a selected police station,
    // including its associated police officers.
    // GET: POLICESTATIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policestation = await _context.PoliceStations
            .Include(m => m.PoliceOfficers)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (policestation == null)
        {
            return NotFound();
        }

        return View(policestation);
    }

    // Displays the form for registering a new police station.
    // GET: POLICESTATIONS/Create
    public IActionResult Create()
    {
        return View();
    }

    // Validates and saves a new police station to the database.
    // POST: POLICESTATIONS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,StationCode,Location,Province,ContactNumber,StationCommander")] 
    PoliceStation policestation)
    {
        if (ModelState.IsValid)
        {
            _context.Add(policestation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(policestation);
    }

    // Retrieves an existing police station for editing.
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

    // Validates and updates the selected police station in the database.
    // POST: POLICESTATIONS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id, 
        [Bind("Id,Name,StationCode,Location,Province,ContactNumber,StationCommander")]
    PoliceStation policestation)
    {
        if (id != policestation.Id)
        {
            return NotFound();
        }

        if (!PoliceStationExists(id))
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

    // Retrieves and displays a police station before deletion.
    // GET: POLICESTATIONS/Delete/5
    [HttpGet]
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

    // Removes the selected police station from the database.
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

    // Checks whether a police station exists using its ID.
    private bool PoliceStationExists(int? id)
    {
        return _context.PoliceStations.Any(e => e.Id == id);
    }
}
