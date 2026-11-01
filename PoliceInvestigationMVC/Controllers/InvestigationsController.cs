
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Exceptions;
using PoliceInvestigationMVC.Models;
using System;
using System.Linq;


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
             .OrderByDescending(i => i.DateCreated)
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
        if (!await _context.PoliceOfficers
            .AnyAsync(o => o.Id == investigation.InvestigatingOfficerId))
        {
            ModelState.AddModelError(
                "InvestigatingOfficerId",
                "Please select a valid investigating officer.");
        }

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
        [Bind("Id,CaseNumber,Title,Description,Status,CaseType,Priority,DateCreated,InvestigatingOfficerId,IsReviewed")] 
    Investigation investigation)
    {
        if (id != investigation.Id)
        {
            return NotFound();
        }

        if (!await _context.PoliceOfficers
            .AnyAsync(o => o.Id == investigation.InvestigatingOfficerId))
        {
            ModelState.AddModelError(
                "InvestigatingOfficerId",
                "Please select a valid investigating officer.");
        }

        var allowedStatuses = new[] { "Open", "In Progress", "Closed" };

        if (!allowedStatuses.Contains(investigation.Status))
        {
            ModelState.AddModelError(
                "Status",
                "Please select a valid investigation status.");
        }

        if (investigation.Status == "Closed" && !investigation.IsReviewed)
        {
            ModelState.AddModelError(
                "IsReviewed",
                "The investigation must be reviewed before it can be closed.");
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

    public async Task<IActionResult> CaseSummary()
    {
        var investigations = await _context.Investigations
            .Select(i => new
            {
                i.CaseNumber,
                i.Title,
                i.Status
            })
            .ToListAsync();

        return Json(investigations);
    }

    public async Task<IActionResult> OpenCaseCount()
    {
        int count = await _context.Investigations
            .CountAsync(i => i.Status == "Open");

        return Content($"Total open investigations: {count}");
    }

    public async Task<IActionResult> HasOpenCases()
    {
        bool hasOpenCases = await _context.Investigations
            .AnyAsync(i => i.Status == "Open");

        return Content(
            hasOpenCases
                ? "Open investigations exist."
                : "No open investigations found.");
    }

    public async Task<IActionResult> CheckCaseNumbers()
    {
        bool allHaveCaseNumbers = await _context.Investigations
            .AllAsync(i => !string.IsNullOrWhiteSpace(i.CaseNumber));

        return Content(
            allHaveCaseNumbers
                ? "All investigations have a case number."
                : "Some investigations are missing a case number.");
    }

    public async Task<IActionResult> CasesByStatus()
    {
        var cases = await _context.Investigations
            .GroupBy(i => i.Status)
            .Select(g => new
            {
                Status = g.Key,
                Total = g.Count()
            })
            .ToListAsync();

        return Json(cases);
    }

    public async Task<IActionResult> AverageCaseAge()
    {
        var dates = await _context.Investigations
            .Select(i => i.DateCreated)
            .ToListAsync();

        if (dates.Count == 0)
        {
            return Content("There are no investigations to calculate.");
        }

        double averageDays = dates
            .Average(date => (DateTime.Now - date).TotalDays);

        return Content(
            $"Average investigation age: {averageDays:F1} days");
    }

    private void ValidateInvestigationClosure(
    string status,
    bool reviewed)
    {
        if (status != "Open")
        {
            throw new InvestigationException(
                "Only open investigations can be closed.");
        }

        if (!reviewed)
        {
            throw new InvestigationException(
                "The investigation must be reviewed first.");
        }
    }

    public async Task<IActionResult> TestGeneric()
    {
        var investigation = await _context.Investigations
            .FirstOrDefaultAsync();

        var result = new OperationResult<Investigation>
        {
            Success = investigation != null,
            Message = investigation != null
                ? "Investigation found successfully."
                : "No investigations found.",
            Data = investigation
        };

        return Json(result);
    }

    private delegate bool InvestigationCheck(Investigation investigation);

    public async Task<IActionResult> TestDelegate()
    {
        var investigation = await _context.Investigations
            .FirstOrDefaultAsync();

        if (investigation == null)
        {
            return Content("No investigations found.");
        }

        InvestigationCheck isHighPriority =
            i => i.Priority == "High";

        bool result = isHighPriority(investigation);

        return Content(
            result
                ? "This investigation has high priority."
                : "This investigation does not have high priority.");
    }

    public IActionResult TestInvestigationEvent()
    {
        var service = new InvestigationEventService();

        string message = "No event received.";

        service.InvestigationCreated += (sender, investigation) =>
        {
            message = $"Investigation created: {investigation.CaseNumber}";
        };

        var testInvestigation = new Investigation
        {
            CaseNumber = "TEST-001",
            Title = "Test Investigation",
            Status = "Open",
            Priority = "High",
            CaseType = "Test",
            Description = "Testing the investigation event."
        };

        service.NotifyInvestigationCreated(testInvestigation);

        return Content(message);
    }

    public IActionResult TestInvestigationException()
    {
        try
        {
            ValidateInvestigationClosure("Open", false);

            return Content("Investigation passed validation.");
        }
        catch (InvestigationException ex)
        {
            return Content($"Business rule error: {ex.Message}");
        }
    }

    public async Task<IActionResult> HighPriorityCases()
    {
        InvestigationCheck isHighPriority =
            i => i.Priority == "High";

        var investigations = await _context.Investigations
            .ToListAsync();

        var highPriorityCases = investigations
            .Where(i => isHighPriority(i))
            .ToList();

        return Json(highPriorityCases);
    }

    public async Task<IActionResult> ClosedCases()
    {
        var closedCases = await _context.Investigations
            .Where(i => i.Status == "Closed")
            .ToListAsync();

        return Json(closedCases);
    }

}
