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

    // Loads the investigation and police officer dropdown lists.
    private async Task PopulateDropdownsAsync(
        int? investigationId = null,
        int? collectedById = null)
    {
        ViewData["InvestigationId"] = new SelectList(
            await _context.Investigations.ToListAsync(),
            "Id",
            "CaseNumber",
            investigationId);

        ViewData["CollectedById"] = new SelectList(
            await _context.PoliceOfficers.ToListAsync(),
            "Id",
            "Name",
            collectedById);
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
        var evidenceRecords = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .ToListAsync();

        return View(evidenceRecords);
    }

    // Displays the details of a selected evidence record.
    // GET: Evidences/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var evidence = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .Include(e => e.EvidenceFiles)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
            return NotFound();

        return View(evidence);
    }

    // Displays the form for registering new evidence.
    // GET: Evidences/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    // Validates and saves a new evidence record, including multiple uploaded files.
    // POST: Evidences/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Description,EvidenceType,DateCollected,CollectedById,StorageLocation,ChainOfCustody,InvestigationId")]
    Evidence evidence,
        List<IFormFile> evidenceFiles)
    {
        var allowedExtensions = new[]
        {
        ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"
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
            return NotFound();

        var evidence = await _context.EvidenceRecords
            .Include(e => e.EvidenceFiles)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
            return NotFound();

        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

        return View(evidence);
    }

    // Validates and updates the selected evidence record.
    // POST: Evidences/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Description,EvidenceType,DateCollected,CollectedById,StorageLocation,ChainOfCustody,InvestigationId")]
    Evidence evidence,
    List<IFormFile> evidenceFiles)
    {
        if (id != evidence.Id)
            return NotFound();

        var allowedExtensions = new[]
        {
        ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"
    };

        // Validate each additional uploaded file.
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

        ModelState.Remove(nameof(Evidence.EvidenceReference));

        if (ModelState.IsValid)
        {
            var existingEvidence = await _context.EvidenceRecords
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEvidence == null)
                return NotFound();

            // Update the existing evidence information.
            existingEvidence.Description = evidence.Description;
            existingEvidence.EvidenceType = evidence.EvidenceType;
            existingEvidence.DateCollected = evidence.DateCollected;
            existingEvidence.CollectedById = evidence.CollectedById;
            existingEvidence.StorageLocation = evidence.StorageLocation;
            existingEvidence.ChainOfCustody = evidence.ChainOfCustody;
            existingEvidence.InvestigationId = evidence.InvestigationId;

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

            return RedirectToAction(nameof(Index));
        }

        // Reload dropdowns and existing attachments if validation fails.
        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

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
            return NotFound();

        var evidence = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null)
            return NotFound();

        return View(evidence);
    
}

    // Removes the selected evidence record from the database.
    // POST: Evidences/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var evidence = await _context.EvidenceRecords.FindAsync(id);

        if (evidence != null)
        {
            _context.EvidenceRecords.Remove(evidence);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // Retrieves and downloads the uploaded evidence file.
    public async Task<IActionResult> Download(int id)
    {
        var evidence = await _context.EvidenceRecords
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null || string.IsNullOrEmpty(evidence.FilePath))
            return NotFound();

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            evidence.FilePath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(
            fullPath,
            "application/octet-stream",
            evidence.FileName ?? "EvidenceFile");
    }

    // Downloads an individual file linked to an evidence record.
    public async Task<IActionResult> DownloadAttachment(int fileId)
    {
        var evidenceFile = await _context.EvidenceFiles
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (evidenceFile == null)
            return NotFound();

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            evidenceFile.FilePath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(
            fullPath,
            "application/octet-stream",
            evidenceFile.FileName);
    }
}
