
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;
using PoliceInvestigationMVC.Data;

public class PoliceOfficersController : Controller
{
    // Stores the database context to provide access to police officer records.
    private readonly ApplicationDbContext _context;

    // Receives the database context through dependency injection.
    public PoliceOfficersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Retrieves police officers and their stations.
    // If a station ID is provided, only officers from that station are displayed.
    // GET: POLICEOFFICERS
    public async Task<IActionResult> Index(int? stationId)
    {
        var policeOfficers = _context.PoliceOfficers
            .Include(p => p.Station)
            .AsQueryable();

        if (stationId.HasValue)
        {
            policeOfficers = policeOfficers
                .Where(p => p.StationId == stationId.Value);

            var station = await _context.PoliceStations
                .FirstOrDefaultAsync(s => s.Id == stationId.Value);

            ViewBag.StationName = station?.Name;
        }

        ViewBag.StationId = stationId;

        return View(await policeOfficers.ToListAsync());
    }

    // Retrieves and displays the details of a selected police officer.
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

    // Displays the registration form and loads the police station dropdown.
    // GET: POLICEOFFICERS/Create
    public IActionResult Create()
    {
        ViewData["StationId"] = new SelectList(
            _context.PoliceStations,
            "Id",
            "Name");

        return View();
    }

    // Validates and saves a new police officer to the database.
    // Also checks that the service number is not already registered.
    // POST: POLICEOFFICERS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,ServiceNumber,Name,Rank,StationId,ContactNumber,Email,EmploymentStatus")]
    PoliceOfficer policeofficer)
    {
        // Check whether the service number already exists
        bool serviceNumberExists = await _context.PoliceOfficers
            .AnyAsync(o => o.ServiceNumber == policeofficer.ServiceNumber);

        if (serviceNumberExists)
        {
            ModelState.AddModelError(
                "ServiceNumber",
                "This service number is already registered.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(policeofficer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Reload the police station dropdown if validation fails
        ViewData["StationId"] = new SelectList(
            _context.PoliceStations,
            "Id",
            "Name",
            policeofficer.StationId);

        return View(policeofficer);
    }

    // Retrieves an existing police officer for editing.
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

    // Validates and updates police officer information in the database.
    // Checks for duplicate service numbers and handles database concurrency.
    // POST: POLICEOFFICERS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int? id,
    [Bind("Id,ServiceNumber,Name,Rank,StationId,ContactNumber,Email,EmploymentStatus")]
    PoliceOfficer policeofficer)
    {
        if (id != policeofficer.Id)
        {
            return NotFound();
        }

        // Check whether the service number belongs to another officer.
        bool serviceNumberExists = await _context.PoliceOfficers
            .AnyAsync(o => o.ServiceNumber == policeofficer.ServiceNumber
                        && o.Id != policeofficer.Id);

        if (serviceNumberExists)
        {
            ModelState.AddModelError(
                nameof(policeofficer.ServiceNumber),
                "This service number is already registered.");
        }

        if (ModelState.IsValid)
        {
            var existingOfficer = await _context.PoliceOfficers
                .FirstOrDefaultAsync(o => o.Id == policeofficer.Id);

            if (existingOfficer == null)
            {
                return NotFound();
            }

            // Update the existing officer's information.
            existingOfficer.ServiceNumber = policeofficer.ServiceNumber;
            existingOfficer.Name = policeofficer.Name;
            existingOfficer.Rank = policeofficer.Rank;
            existingOfficer.StationId = policeofficer.StationId;
            existingOfficer.ContactNumber = policeofficer.ContactNumber;
            existingOfficer.Email = policeofficer.Email;
            existingOfficer.EmploymentStatus = policeofficer.EmploymentStatus;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PoliceOfficerExists(policeofficer.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // Reload the station dropdown if validation fails.
        ViewData["StationId"] = new SelectList(
            _context.PoliceStations,
            "Id",
            "Name",
            policeofficer.StationId);

        return View(policeofficer);
    }

    // Retrieves and displays a police officer before deletion.
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

    // Checks whether the officer has investigation or team assignments
    // before allowing the officer's record to be deleted.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policeofficer = await _context.PoliceOfficers
            .Include(o => o.Station)
            .Include(o => o.InvestigationOfficers)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (policeofficer == null)
        {
            return NotFound();
        }

        // Check whether the officer is assigned to any investigations.
        bool hasInvestigations = await _context.Investigations
            .AnyAsync(i => i.InvestigatingOfficerId == id);

        // Check whether the officer belongs to an investigation team.
        bool hasTeamAssignments = policeofficer.InvestigationOfficers.Any();

        if (hasInvestigations || hasTeamAssignments)
        {
            ViewBag.DeleteError =
                "This officer cannot be deleted because they are assigned to investigations or investigation teams. Remove those assignments first.";

            return View(policeofficer);
        }

        _context.PoliceOfficers.Remove(policeofficer);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Checks whether a police officer exists using their ID.
    private bool PoliceOfficerExists(int? id)
    {
        return _context.PoliceOfficers.Any(e => e.Id == id);
    }
}
