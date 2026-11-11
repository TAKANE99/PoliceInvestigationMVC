using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

namespace PoliceInvestigationMVC.Controllers
{
    public class CaseReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaseReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display all case reviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.CaseReviews
                .Include(r => r.Investigation)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Display the Create Review form
        public async Task<IActionResult> Create()
        {
            await LoadInvestigations();
            return View();
        }

        // POST: Save a new case review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseReview review)
        {
            bool investigationExists = await _context.Investigations
                .AnyAsync(i => i.Id == review.InvestigationId);

            if (!investigationExists)
            {
                ModelState.AddModelError(
                    "InvestigationId",
                    "Please select a valid investigation.");
            }

            bool reviewExists = await _context.CaseReviews
                .AnyAsync(r => r.InvestigationId == review.InvestigationId);

            if (reviewExists)
            {
                ModelState.AddModelError(
                    "InvestigationId",
                    "This investigation already has a review.");
            }

            if (ModelState.IsValid)
            {
                review.ReviewDate = DateTime.Now;

                _context.CaseReviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await LoadInvestigations();
            return View(review);
        }

        // GET: Display the Edit Case Review page
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.CaseReviews
                .Include(r => r.Investigation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Save changes to an existing review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CaseReview review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(review);
            }

            var existingReview = await _context.CaseReviews
                .FindAsync(id);

            if (existingReview == null)
            {
                return NotFound();
            }

            existingReview.ReviewComments = review.ReviewComments;
            existingReview.ReviewStatus = review.ReviewStatus;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Display Delete confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.CaseReviews
                .Include(r => r.Investigation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Delete the case review
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.CaseReviews.FindAsync(id);

            if (review != null)
            {
                _context.CaseReviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Load investigations into the dropdown
        private async Task LoadInvestigations()
        {
            ViewData["InvestigationId"] = new SelectList(
                await _context.Investigations.ToListAsync(),
                "Id",
                "CaseNumber");
        }
    }
}