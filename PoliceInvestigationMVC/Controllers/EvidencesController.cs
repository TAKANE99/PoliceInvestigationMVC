
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

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
    public async Task<IActionResult> Create([Bind("Id,Description,EvidenceType,InvestigationId,Investigation")] Evidence evidence)
    {
        if (ModelState.IsValid)
        {
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
}
