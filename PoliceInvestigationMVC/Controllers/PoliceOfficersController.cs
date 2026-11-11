
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

    private bool PoliceOfficerExists(int? id)
    {
        return _context.PoliceOfficers.Any(e => e.Id == id);
    }
}
