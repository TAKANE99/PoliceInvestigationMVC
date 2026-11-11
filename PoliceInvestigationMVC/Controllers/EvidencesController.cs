using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

[Authorize]
public class EvidencesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EvidencesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Load both dropdowns.
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

    // Generate evidence reference automatically.
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

    // GET: Evidences
    public async Task<IActionResult> Index()
    {
        var evidenceRecords = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .Include(e => e.CollectedBy)
            .ToListAsync();

        return View(evidenceRecords);
    }

    // GET: Evidences/Details/5
    public async Task<IActionResult> Details(int? id)
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

    // GET: Evidences/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    // POST: Evidences/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Description,EvidenceType,DateCollected,CollectedById,StorageLocation,ChainOfCustody,InvestigationId")]
        Evidence evidence,
        IFormFile? evidenceFile)
    {
        // Validate uploaded file.
        if (evidenceFile != null && evidenceFile.Length > 0)
        {
            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"
            };

            var extension = Path.GetExtension(evidenceFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "evidenceFile",
                    "Only JPG, JPEG, PNG, PDF, DOC and DOCX files are allowed.");
            }
            else if (evidenceFile.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "evidenceFile",
                    "The file must not exceed 10 MB.");
            }
        }

        ModelState.Remove(nameof(Evidence.EvidenceReference));

        if (ModelState.IsValid)
        {
            // Generate the evidence reference.
            evidence.EvidenceReference =
                await GenerateEvidenceReferenceAsync(
                    evidence.DateCollected!.Value.Year);

            // Save uploaded file, if provided.
            if (evidenceFile != null && evidenceFile.Length > 0)
            {
                var extension = Path.GetExtension(evidenceFile.FileName);

                var uploadFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data",
                    "EvidenceUploads");

                Directory.CreateDirectory(uploadFolder);

                var storedFileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadFolder, storedFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await evidenceFile.CopyToAsync(stream);
                }

                evidence.FileName = Path.GetFileName(evidenceFile.FileName);
                evidence.FilePath = Path.Combine(
                    "App_Data",
                    "EvidenceUploads",
                    storedFileName);
            }

            _context.EvidenceRecords.Add(evidence);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

        return View(evidence);
    }

    // GET: Evidences/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var evidence = await _context.EvidenceRecords.FindAsync(id);

        if (evidence == null)
            return NotFound();

        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

        return View(evidence);
    }

    // POST: Evidences/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Description,EvidenceType,DateCollected,CollectedById,StorageLocation,ChainOfCustody,InvestigationId")]
    Evidence evidence)
    {
        if (id != evidence.Id)
            return NotFound();

        ModelState.Remove(nameof(Evidence.EvidenceReference));

        if (ModelState.IsValid)
        {
            var existingEvidence = await _context.EvidenceRecords
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEvidence == null)
                return NotFound();

            existingEvidence.Description = evidence.Description;
            existingEvidence.EvidenceType = evidence.EvidenceType;
            existingEvidence.DateCollected = evidence.DateCollected;
            existingEvidence.CollectedById = evidence.CollectedById;
            existingEvidence.StorageLocation = evidence.StorageLocation;
            existingEvidence.ChainOfCustody = evidence.ChainOfCustody;
            existingEvidence.InvestigationId = evidence.InvestigationId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdownsAsync(
            evidence.InvestigationId,
            evidence.CollectedById);

        return View(evidence);
    }

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
}
