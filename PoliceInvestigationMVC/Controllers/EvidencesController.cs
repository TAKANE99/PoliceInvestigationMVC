using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

// Restricts access to this controller to authenticated users.
[Authorize]
public class EvidencesController : Controller
{
    // Provides access to the application's database.
    private readonly ApplicationDbContext _context;

    // Injects the database context into the controller.
    public EvidencesController(ApplicationDbContext context)
    {
        _context = context;
    }
    // Finds the police officer linked to the logged-in account.
    private async Task<PoliceOfficer?> GetLoggedInOfficerAsync()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _context.PoliceOfficers
            .FirstOrDefaultAsync(o => o.ApplicationUserId == userId);
    }

    // Loads the investigation and police officer dropdown lists.
    private async Task PopulateDropdownsAsync(
    int? investigationId = null,
    int? collectedById = null)
    {
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            ViewData["InvestigationId"] = new SelectList(
                Enumerable.Empty<Investigation>(),
                "Id",
                "CaseNumber");

            ViewData["CollectedById"] = new SelectList(
                Enumerable.Empty<PoliceOfficer>(),
                "Id",
                "Name");

            return;
        }

        var assignedInvestigations = await _context.Investigations
            .Where(i => i.InvestigatingOfficerId == officer.Id)
            .ToListAsync();

        ViewData["InvestigationId"] = new SelectList(
            assignedInvestigations,
            "Id",
            "CaseNumber",
            investigationId);

        ViewData["CollectedById"] = new SelectList(
            new[] { officer },
            "Id",
            "Name",
            collectedById ?? officer.Id);
    }

    // Generates a unique evidence reference using the collection year and next available number.
    private async Task<string> GenerateEvidenceReferenceAsync(int year)
    {
        string prefix = $"EVD-{year}-";

        var references = await _context.EvidenceRecords
            .Where(e => e.EvidenceReference.StartsWith(prefix))
            .Select(e => e.EvidenceReference)
            .ToListAsync();

        int highestNumber = references
            .Select(r => int.TryParse(r.Substring(prefix.Length), out int number)
                ? number : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{highestNumber + 1:D3}";
    }

    // Displays all evidence records linked to a selected investigation.
    // GET: Evidences/ByInvestigation/5
    public async Task<IActionResult> ByInvestigation(int investigationId)
    {
        var investigation = await _context.Investigations
            .FirstOrDefaultAsync(i => i.Id == investigationId);

        if (investigation == null)
        {
            return NotFound();
        }

        // Administrators can view evidence for any investigation.
        if (User.IsInRole("Administrator"))
        {
            // Continue below.
        }
        else
        {
            // Officers can only view evidence for their assigned investigations.
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null ||
                investigation.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }
        }

        var evidenceRecords = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .Where(e => e.InvestigationId == investigationId)
            .ToListAsync();

        ViewBag.CaseNumber = investigation.CaseNumber;
        ViewBag.InvestigationId = investigation.Id;

        return View("ByInvestigation", evidenceRecords);
    }

    // Retrieves and displays all evidence records with their investigations and collecting officers.
    // GET: Evidences
    public async Task<IActionResult> Index()
    {
        var evidenceQuery = _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .AsQueryable();

        // Administrators can view all evidence records.
        if (User.IsInRole("Administrator"))
        {
            return View(await evidenceQuery.ToListAsync());
        }

        // Police officers can only view evidence for their assigned cases.
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        var evidenceRecords = await evidenceQuery
            .Where(e => e.Investigation!.InvestigatingOfficerId == officer.Id)
            .ToListAsync();

        return View(evidenceRecords);
    }

    // Displays the details of a selected evidence record.
    // GET: Evidences/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var evidence = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .Include(e => e.EvidenceFiles)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
        {
            return NotFound();
        }

        // Administrators can view any evidence record.
        if (User.IsInRole("Administrator"))
        {
            return View(evidence);
        }

        // Officers can only view evidence from their assigned cases.
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null ||
            evidence.Investigation?.InvestigatingOfficerId != officer.Id)
        {
            return Forbid();
        }

        return View(evidence);
    }
    // Displays the form for registering new evidence.
    // GET: Evidences/Create
    [Authorize(Roles = "PoliceOfficer")]
    public async Task<IActionResult> Create()
    {
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Show only investigations assigned to this officer.
        var assignedInvestigations = await _context.Investigations
            .Where(i => i.InvestigatingOfficerId == officer.Id)
            .ToListAsync();

        ViewData["InvestigationId"] = new SelectList(
            assignedInvestigations,
            "Id",
            "CaseNumber");

        // The officer can only be selected as the collecting officer.
        ViewData["CollectedById"] = new SelectList(
            new[] { officer },
            "Id",
            "Name",
            officer.Id);

        return View();
    }

    // Validates and saves a new evidence record, including multiple uploaded files.
    // POST: Evidences/Create
    [Authorize(Roles = "PoliceOfficer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Description,EvidenceType,DateCollected,CollectedById,StorageLocation,ChainOfCustody,InvestigationId")]
    Evidence evidence,
        List<IFormFile> evidenceFiles)
    {
        // Get the logged-in police officer.
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Confirm that the selected investigation belongs to this officer.
        var assignedInvestigation = await _context.Investigations
            .AnyAsync(i =>
                i.Id == evidence.InvestigationId &&
                i.InvestigatingOfficerId == officer.Id);

        if (!assignedInvestigation)
        {
            return Forbid();
        }

        // Set the collecting officer from the logged-in account.
        evidence.CollectedById = officer.Id;
        ModelState.Remove(nameof(Evidence.CollectedById));

        // Automatically record the collection date and time.
        evidence.DateCollected = DateTime.Now;
        ModelState.Remove(nameof(Evidence.DateCollected));

        var allowedExtensions = new[]
        {
            // Images
            ".jpg", ".jpeg", ".png",

            // Videos
            ".mp4", ".mov", ".avi",

            // Documents
            ".pdf", ".doc", ".docx"
         };

        // Validates every uploaded file.
        if (evidenceFiles != null)
        {
            foreach (var file in evidenceFiles.Where(f => f.Length > 0))
            {
                var extension = Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "evidenceFiles",
                        $"The file {Path.GetFileName(file.FileName)} has an unsupported format.");
                }
                else if (file.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "evidenceFiles",
                        $"The file {Path.GetFileName(file.FileName)} exceeds 10 MB.");
                }
            }
        }

        // The evidence reference is generated automatically.
        ModelState.Remove(nameof(Evidence.EvidenceReference));

        if (ModelState.IsValid)
        {
            evidence.EvidenceReference =
                await GenerateEvidenceReferenceAsync(
                    evidence.DateCollected!.Value.Year);

            // Save the main evidence record first to generate its ID.
            _context.EvidenceRecords.Add(evidence);
            await _context.SaveChangesAsync();

            // Save each uploaded file separately.
            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "App_Data",
                "EvidenceUploads");

            Directory.CreateDirectory(uploadFolder);

            if (evidenceFiles != null)
            {
                foreach (var file in evidenceFiles.Where(f => f.Length > 0))
                {
                    var extension = Path.GetExtension(file.FileName)
                        .ToLowerInvariant();

                    var storedFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var evidenceFile = new EvidenceFile
                    {
                        EvidenceId = evidence.Id,
                        FileName = Path.GetFileName(file.FileName),
                        FilePath = Path.Combine(
                            "App_Data",
                            "EvidenceUploads",
                            storedFileName),
                        DateUploaded = DateTime.Now
                    };

                    _context.EvidenceFiles.Add(evidenceFile);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Reload dropdowns if validation fails.
        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

        return View(evidence);
    }

    // Finds an existing evidence record and displays the edit form.
    // GET: Evidences/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Only police officers may edit evidence records.
        if (!User.IsInRole("PoliceOfficer"))
        {
            return Forbid();
        }

        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        var evidence = await _context.EvidenceRecords
            .Include(e => e.EvidenceFiles)
            .Include(e => e.Investigation)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
        {
            return NotFound();
        }

        // Officers may only edit evidence they collected for their assigned cases.
        if (evidence.Investigation?.InvestigatingOfficerId != officer.Id)
        {
            return Forbid();
        }

        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

        return View(evidence);
    }

    // Validates and updates the selected evidence record.
    // POST: Evidences/Edit/5
    [Authorize(Roles = "PoliceOfficer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
     int id,
     [Bind("Id,Description,EvidenceType,DateCollected,StorageLocation,ChainOfCustody")]
    Evidence evidence,
     List<IFormFile> evidenceFiles)
    {
        // Check that the submitted ID matches the URL ID.
        if (id != evidence.Id)
        {
            return NotFound();
        }

        // Get the logged-in police officer.
        var officer = await GetLoggedInOfficerAsync();

        if (officer == null)
        {
            return Forbid();
        }

        // Retrieve the original evidence record.
        var existingEvidence = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (existingEvidence == null)
        {
            return NotFound();
        }

        // Confirm that the evidence belongs to the officer's assigned case.
        if (existingEvidence.Investigation?.InvestigatingOfficerId != officer.Id)
        {
            return Forbid();
        }

        var allowedExtensions = new[]
        {
            ".jpg",".jpeg",".png",".mp4",".mov",".avi",".pdf",".doc",".docx"
        };

        // Validate uploaded attachments.
        if (evidenceFiles != null)
        {
            foreach (var file in evidenceFiles.Where(f => f.Length > 0))
            {
                var extension = Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "evidenceFiles",
                        $"{Path.GetFileName(file.FileName)} has an unsupported format.");
                }
                else if (file.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "evidenceFiles",
                        $"{Path.GetFileName(file.FileName)} exceeds 10 MB.");
                }
            }
        }

        // Preserve the original collecting officer.
        evidence.CollectedById = existingEvidence.CollectedById;

        // Preserve the original investigation link.
        evidence.InvestigationId = existingEvidence.InvestigationId;

        // Remove validation errors for fields that cannot be changed.
        ModelState.Remove(nameof(Evidence.CollectedById));
        ModelState.Remove(nameof(Evidence.InvestigationId));
        ModelState.Remove(nameof(Evidence.EvidenceReference));

        if (ModelState.IsValid)
        {
            // Update the existing evidence information.
            existingEvidence.Description = evidence.Description;
            existingEvidence.EvidenceType = evidence.EvidenceType;
            existingEvidence.DateCollected = evidence.DateCollected;
            existingEvidence.StorageLocation = evidence.StorageLocation;
            existingEvidence.ChainOfCustody = evidence.ChainOfCustody;

            // Save any newly uploaded attachments.
            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "App_Data",
                "EvidenceUploads");

            Directory.CreateDirectory(uploadFolder);

            if (evidenceFiles != null)
            {
                foreach (var file in evidenceFiles.Where(f => f.Length > 0))
                {
                    var extension = Path.GetExtension(file.FileName)
                        .ToLowerInvariant();

                    var storedFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var newEvidenceFile = new EvidenceFile
                    {
                        EvidenceId = existingEvidence.Id,
                        FileName = Path.GetFileName(file.FileName),
                        FilePath = Path.Combine(
                            "App_Data",
                            "EvidenceUploads",
                            storedFileName),
                        DateUploaded = DateTime.Now
                    };

                    _context.EvidenceFiles.Add(newEvidenceFile);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Evidence updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Reload the dropdowns and existing attachments if validation fails.
        await PopulateDropdownsAsync(
            existingEvidence.InvestigationId,
            existingEvidence.CollectedById);

        evidence.EvidenceFiles = await _context.EvidenceFiles
            .Where(f => f.EvidenceId == id)
            .ToListAsync();

        return View(evidence);
    }


    // Displays the evidence record before it is deleted.
    // GET: Evidences/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var evidence = await _context.EvidenceRecords
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
        {
            return NotFound();
        }

        TempData["ErrorMessage"] =
            "Deletion is not permitted. Evidence records must be retained to preserve investigation history and chain of custody.";

        return RedirectToAction(nameof(Index));
    }

    // Removes the selected evidence record from the database.
    // POST: Evidences/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var evidence = await _context.EvidenceRecords
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
        {
            return NotFound();
        }

        TempData["ErrorMessage"] =
            "Deletion is not permitted. Evidence records must be retained to preserve investigation history and chain of custody.";

        return RedirectToAction(nameof(Index));
    }

    // Retrieves and downloads the uploaded evidence file.
    public async Task<IActionResult> Download(int id)
    {
        var evidence = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null || string.IsNullOrEmpty(evidence.FilePath))
        {
            return NotFound();
        }

        // Administrators can download any evidence file.
        if (!User.IsInRole("Administrator"))
        {
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null ||
                evidence.Investigation?.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }
        }

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            evidence.FilePath);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        return PhysicalFile(
            fullPath,
            "application/octet-stream",
            evidence.FileName ?? "EvidenceFile");
    }

    // Downloads an individual file linked to an evidence record.
    public async Task<IActionResult> DownloadAttachment(int fileId)
    {
        var result = await _context.EvidenceFiles
            .Join(
                _context.EvidenceRecords.Include(e => e.Investigation),
                file => file.EvidenceId,
                evidence => evidence.Id,
                (file, evidence) => new
                {
                    File = file,
                    Evidence = evidence
                })
            .FirstOrDefaultAsync(x => x.File.Id == fileId);

        if (result == null)
        {
            return NotFound();
        }

        // Administrators can download any evidence attachment.
        if (!User.IsInRole("Administrator"))
        {
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null ||
                result.Evidence.Investigation?.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }
        }

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            result.File.FilePath);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        return PhysicalFile(
            fullPath,
            "application/octet-stream",
            result.File.FileName);
    }
}
