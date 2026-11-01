
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class EvidencesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EvidencesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: EVIDENCES
    public async Task<IActionResult> Index()    
    {
        var evidenceRecords = await _context.EvidenceRecords
        .Include(e => e.Investigation)
        .ToListAsync();

        return View(evidenceRecords);
    }

    // GET: EVIDENCES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var evidence = await _context.EvidenceRecords
            .Include(e => e.Investigation)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (evidence == null)
        {
            return NotFound();
        }

        return View(evidence);
    }

    // GET: EVIDENCES/Create
    public IActionResult Create()
    {
        ViewData["InvestigationId"] = new SelectList(
        _context.Investigations,
        "Id",
        "CaseNumber");

        return View();
    }

    // POST: EVIDENCES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Description,EvidenceType,InvestigationId")] 
    Evidence evidence, IFormFile? evidenceFile)
    {
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
            else
            {
                evidence.FileName = Path.GetFileName(evidenceFile.FileName);
            }
        }
        if (ModelState.IsValid)
        {
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
                    "App_Data", "EvidenceUploads", storedFileName);
                evidence.DateCollected = DateTime.Now;
            }

            _context.Add(evidence);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        ViewData["InvestigationId"] = new SelectList(
            _context.Investigations,
            "Id",
            "CaseNumber",
            evidence.InvestigationId);

        return View(evidence);
    }

    // GET: EVIDENCES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var evidence = await _context.EvidenceRecords.FindAsync(id);

        if (evidence == null)
        {
            return NotFound();
        }
        ViewData["InvestigationId"] = new SelectList(
        _context.Investigations,
        "Id",
        "CaseNumber",
        evidence.InvestigationId);

        return View(evidence);
    }

    // POST: EVIDENCES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id, 
        [Bind("Id,Description,EvidenceType,InvestigationId")] 
        Evidence evidence)
    {
        if (id != evidence.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(evidence);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EvidenceExists(evidence.Id))
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
        ViewData["InvestigationId"] = new SelectList(
        _context.Investigations,
        "Id",
        "CaseNumber",
        evidence.InvestigationId);

        return View(evidence);
    }

    // GET: EVIDENCES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var evidence = await _context.EvidenceRecords
            .FirstOrDefaultAsync(m => m.Id == id);
        if (evidence == null)
        {
            return NotFound();
        }

        return View(evidence);
    }

    // POST: EVIDENCES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var evidence = await _context.EvidenceRecords.FindAsync(id);
        if (evidence != null)
        {
            _context.EvidenceRecords.Remove(evidence);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EvidenceExists(int? id)
    {
        return _context.EvidenceRecords.Any(e => e.Id == id);
    }

    public async Task<IActionResult> Download(int id)
    {
        var evidence = await _context.EvidenceRecords
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evidence == null || string.IsNullOrEmpty(evidence.FilePath))
        {
            return NotFound();
        }

        var fullPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            evidence.FilePath);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var contentType = "application/octet-stream";

        return PhysicalFile(
            fullPath,
            contentType,
            evidence.FileName ?? "EvidenceFile");
    }
}
