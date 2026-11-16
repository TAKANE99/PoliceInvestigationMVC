
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Exceptions;
using PoliceInvestigationMVC.Models;
using System;
using System.Linq;

// Handles investigation-related operations, including creating,
// viewing, updating, deleting and validating investigations.
public class InvestigationsController : Controller
{
    // Initializes the controller and provides access to the application database.
    private readonly ApplicationDbContext _context;
    private readonly string[] _crimeCategories = {
        "Theft", "Robbery","Assault","Murder","Fraud","Housebreaking",
        "Drug-related offence","Other" };

    public InvestigationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Retrieves all investigations or filters them by a selected police officer.
    // Includes officers assigned as main investigators and investigation team members.
    // GET: INVESTIGATIONS
    public async Task<IActionResult> Index(int? officerId)
    {
        var investigations = _context.Investigations
            .Include(i => i.InvestigatingOfficer)
            .AsQueryable();

        // Filter investigations assigned to the selected officer.
        if (officerId.HasValue)
        {
            investigations = investigations.Where(i =>
                i.InvestigatingOfficerId == officerId.Value ||
                _context.InvestigationOfficers.Any(io =>
                    io.InvestigationId == i.Id &&
                    io.PoliceOfficerId == officerId.Value));
        }

        // Display the newest investigations first.
        var result = await investigations
            .OrderByDescending(i => i.DateCreated)
            .ToListAsync();

        return View(result);
    }

    // Retrieves and displays the details of a selected investigation.
    // GET: INVESTIGATIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var investigation = await _context.Investigations
            .Include(i => i.InvestigatingOfficer)
            .Include(i => i.InvestigationOfficers)
            .ThenInclude(io => io.PoliceOfficer)
            .Include(i => i.InvestigationSuspects)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (investigation == null)
        {
            return NotFound();
        }

        return View(investigation);
    }

    // Displays the investigation registration form and loads the officer options.
    // GET: Investigations/Create
    public IActionResult Create()
    {
        ViewData["InvestigatingOfficerId"] = new SelectList(
            _context.PoliceOfficers,
            "Id",
            "Name");

        // Loads officers for the multiple-selection team list.
        ViewData["AvailableOfficers"] = new MultiSelectList(
            _context.PoliceOfficers,
            "Id",
            "Name");

        ViewData["CrimeCategory"] = new SelectList(_crimeCategories);

        return View(new Investigation
        {
            Status = "Open",
            DateCreated = DateTime.Now
        });
    }

    // Validates and saves an investigation and its additional team members.
    // POST: Investigations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("CASNumber,Complainant,CrimeCategory,Title,Description,CaseType,Priority,InvestigatingOfficerId")]
    Investigation investigation,
        int[]? selectedOfficerIds)
    {
        // Check that the main investigating officer exists.
        bool officerExists = await _context.PoliceOfficers
            .AnyAsync(o => o.Id == investigation.InvestigatingOfficerId);

        if (!officerExists)
        {
            ModelState.AddModelError(
                "InvestigatingOfficerId",
                "Please select a valid investigating officer.");
        }

        // Remove duplicate selections and exclude the main investigating officer.
        var teamOfficerIds = (selectedOfficerIds ?? Array.Empty<int>())
            .Distinct()
            .Where(id => id != investigation.InvestigatingOfficerId)
            .ToArray();

        // Check that every selected team member exists.
        int validOfficerCount = await _context.PoliceOfficers
            .CountAsync(o => teamOfficerIds.Contains(o.Id));

        if (validOfficerCount != teamOfficerIds.Length)
        {
            ModelState.AddModelError(
                "",
                "One or more selected team members are invalid.");
        }

        ModelState.Remove(nameof(Investigation.CaseNumber));

        if (ModelState.IsValid)
        {
            investigation.Status = "Open";
            investigation.IsReviewed = false;
            investigation.DateCreated = DateTime.Now;

            // Save the investigation first to generate its database ID.
            investigation.CaseNumber = $"TEMP-{Guid.NewGuid():N}";

            _context.Investigations.Add(investigation);
            await _context.SaveChangesAsync();

            // Generate the final case number.
            investigation.CaseNumber =
                $"INV-{DateTime.Now.Year}-{investigation.Id:D3}";

            await _context.SaveChangesAsync();

            // Save each additional officer against this investigation.
            foreach (int officerId in teamOfficerIds)
            {
                _context.InvestigationOfficers.Add(new InvestigationOfficer
                {
                    InvestigationId = investigation.Id,
                    PoliceOfficerId = officerId,
                    TeamRole = "Team Member",
                    AssignedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Reload dropdown options if validation fails.
        ViewData["InvestigatingOfficerId"] = new SelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            investigation.InvestigatingOfficerId);

        ViewData["AvailableOfficers"] = new MultiSelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            teamOfficerIds);

        ViewData["CrimeCategory"] = new SelectList(
            _crimeCategories,
            investigation.CrimeCategory);

        return View(investigation);
    }

    // Retrieves an existing investigation and its assigned team for editing.
    // GET: INVESTIGATIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var investigation = await _context.Investigations
            .FirstOrDefaultAsync(i => i.Id == id);

        if (investigation == null)
        {
            return NotFound();
        }

        // Get the IDs of officers already assigned to this investigation.
        var selectedOfficerIds = await _context.InvestigationOfficers
            .Where(io => io.InvestigationId == investigation.Id)
            .Select(io => io.PoliceOfficerId)
            .ToListAsync();

        ViewData["InvestigatingOfficerId"] = new SelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            investigation.InvestigatingOfficerId);

        // Load additional team members and keep existing selections selected.
        ViewData["AvailableOfficers"] = new MultiSelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            selectedOfficerIds);

        ViewData["CrimeCategory"] = new SelectList(
            _crimeCategories,
            investigation.CrimeCategory);

        return View(investigation);
    }

    // Validates and updates an investigation and its assigned team.
    // POST: INVESTIGATIONS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,
        [Bind("Id,CaseNumber,CASNumber,Complainant,CrimeCategory,Title,Description,Status,CaseType,Priority,DateCreated,InvestigatingOfficerId,IsReviewed")]
    Investigation investigation,
        int[]? selectedOfficerIds)
    {
        if (id != investigation.Id)
        {
            return NotFound();
        }

        // Validate the main investigating officer.
        if (!await _context.PoliceOfficers
            .AnyAsync(o => o.Id == investigation.InvestigatingOfficerId))
        {
            ModelState.AddModelError(
                "InvestigatingOfficerId",
                "Please select a valid investigating officer.");
        }

        // Remove duplicates and prevent the main officer from being added twice.
        var teamOfficerIds = (selectedOfficerIds ?? Array.Empty<int>())
            .Distinct()
            .Where(officerId => officerId != investigation.InvestigatingOfficerId)
            .ToArray();

        // Validate all additional team members.
        int validOfficerCount = await _context.PoliceOfficers
            .CountAsync(o => teamOfficerIds.Contains(o.Id));

        if (validOfficerCount != teamOfficerIds.Length)
        {
            ModelState.AddModelError(
                "",
                "One or more selected team members are invalid.");
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
                // Retrieve the existing database record.
                var existingInvestigation = await _context.Investigations
                    .FirstOrDefaultAsync(i => i.Id == investigation.Id);

                if (existingInvestigation == null)
                {
                    return NotFound();
                }

                // Update the investigation's existing details.
                existingInvestigation.CaseNumber = investigation.CaseNumber;
                existingInvestigation.CASNumber = investigation.CASNumber;
                existingInvestigation.Complainant = investigation.Complainant;
                existingInvestigation.CrimeCategory = investigation.CrimeCategory;
                existingInvestigation.Title = investigation.Title;
                existingInvestigation.Description = investigation.Description;
                existingInvestigation.Status = investigation.Status;
                existingInvestigation.CaseType = investigation.CaseType;
                existingInvestigation.Priority = investigation.Priority;
                existingInvestigation.DateCreated = investigation.DateCreated;
                existingInvestigation.InvestigatingOfficerId = investigation.InvestigatingOfficerId;
                existingInvestigation.IsReviewed = investigation.IsReviewed;

                // Retrieve the existing additional team assignments.
                var existingTeam = await _context.InvestigationOfficers
                    .Where(io => io.InvestigationId == investigation.Id)
                    .ToListAsync();

                // Remove officers who are no longer selected.
                var officersToRemove = existingTeam
                    .Where(io => !teamOfficerIds.Contains(io.PoliceOfficerId))
                    .ToList();

                _context.InvestigationOfficers.RemoveRange(officersToRemove);

                // Add newly selected officers without duplicating existing assignments.
                var existingOfficerIds = existingTeam
                    .Select(io => io.PoliceOfficerId)
                    .ToHashSet();

                foreach (int officerId in teamOfficerIds)
                {
                    if (!existingOfficerIds.Contains(officerId))
                    {
                        _context.InvestigationOfficers.Add(new InvestigationOfficer
                        {
                            InvestigationId = investigation.Id,
                            PoliceOfficerId = officerId,
                            TeamRole = "Team Member",
                            AssignedDate = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvestigationExists(investigation.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Details), new { id = investigation.Id });
        }

        // Reload dropdowns if validation fails.
        ViewData["InvestigatingOfficerId"] = new SelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            investigation.InvestigatingOfficerId);

        ViewData["AvailableOfficers"] = new MultiSelectList(
            _context.PoliceOfficers,
            "Id",
            "Name",
            teamOfficerIds);

        ViewData["CrimeCategory"] = new SelectList(
            _crimeCategories,
            investigation.CrimeCategory);

        return View(investigation);
    }

    // Displays the investigation details before deletion.
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

    // Removes the selected investigation from the database.
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

    // Checks whether an investigation exists in the database using its ID.
    private bool InvestigationExists(int? id)
    {
        return _context.Investigations.Any(e => e.Id == id);
    }

    // Provides investigation summaries and statistics using LINQ queries.
    //Returns a summary of investigations.
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

    //Counts investigations with Open status.
    public async Task<IActionResult> OpenCaseCount()
    {
        int count = await _context.Investigations
            .CountAsync(i => i.Status == "Open");

        return Content($"Total open investigations: {count}");
    }

    //Checks whether any open investigations exist.
    public async Task<IActionResult> HasOpenCases()
    {
        bool hasOpenCases = await _context.Investigations
            .AnyAsync(i => i.Status == "Open");

        return Content(
            hasOpenCases
                ? "Open investigations exist."
                : "No open investigations found.");
    }

    //Checks whether all investigations have case numbers.
    public async Task<IActionResult> CheckCaseNumbers()
    {
        bool allHaveCaseNumbers = await _context.Investigations
            .AllAsync(i => !string.IsNullOrWhiteSpace(i.CaseNumber));

        return Content(
            allHaveCaseNumbers
                ? "All investigations have a case number."
                : "Some investigations are missing a case number.");
    }

    //	Groups investigations according to their status.
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

    //	Calculates the average age of investigations in days.
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

    // Validates investigation closure using the application's business rules.
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

    // Demonstrates the use of generics to return investigation results.
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

    // Defines a delegate that checks whether an investigation meets a condition.
    private delegate bool InvestigationCheck(Investigation investigation);

    // Demonstrates how delegates can be used to check investigation priority.
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

    // Demonstrates how events can notify the application when an investigation is created.
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

    // Demonstrates custom exception handling for investigation business rules.
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

    // Retrieves investigations filtered by priority and status.
    //Retrieves high-priority investigations.
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
    //Retrieves closed investigations.
    public async Task<IActionResult> ClosedCases()
    {
        var closedCases = await _context.Investigations
            .Where(i => i.Status == "Closed")
            .ToListAsync();

        return Json(closedCases);
    }

}
