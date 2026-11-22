using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;

public class PoliceStationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PoliceStationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Displays all registered police stations.
    public async Task<IActionResult> Index()
    {
        return View(await _context.PoliceStations.ToListAsync());
    }

    // Displays details of a selected police station.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var policeStation = await _context.PoliceStations
            .Include(s => s.PoliceOfficers)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (policeStation == null)
        {
            return NotFound();
        }

        return View(policeStation);
    }
}