using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;

namespace PoliceInvestigationMVC.Controllers
{
    // Manages case reviews and their relationship with investigations.
    public class CaseReviewsController : Controller
    {
        // Provides access to the application's database.
        private readonly ApplicationDbContext _context;

        // Injects the database context into the controller.
        public CaseReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieves and displays all case reviews together with their investigations.
        // GET: Display all case reviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.CaseReviews
                .Include(r => r.Investigation)
                .ToListAsync();

            return View(reviews);
        }

        // Displays the form for creating a new case review.
        // GET: Display the Create Review form
        public async Task<IActionResult> Create()
        {
            await LoadInvestigations();
            return View();
        }
        // Validates and saves a new case review while ensuring the investigation does not already have a review.
        // POST: Save a new case review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseReview review)
        {
            // Checks whether the selected investigation exists in the database.
            bool investigationExists = await _context.Investigations
                .AnyAsync(i => i.Id == review.InvestigationId);

            if (!investigationExists)
            {
                ModelState.AddModelError(
                    "InvestigationId",
                    "Please select a valid investigation.");
            }

            // Checks whether the selected investigation already has a case review.
            bool reviewExists = await _context.CaseReviews
                .AnyAsync(r => r.InvestigationId == review.InvestigationId);

            if (reviewExists)
            {
                ModelState.AddModelError(
                    "InvestigationId",
                    "This investigation already has a review.");
            }

            // Saves the review if all validation checks pass.
            if (ModelState.IsValid)
            {
                review.ReviewDate = DateTime.Now;

                _context.CaseReviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reloads the investigation dropdown if validation fails.
            await LoadInvestigations();
            return View(review);
        }

        // Retrieves the selected case review and displays its edit page.
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

        // Validates and updates the comments and status of an existing case review.
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

            // Retrieves the existing review before updating its information.
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

        // Displays the selected case review before deletion.
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

        // Removes the selected case review from the database.
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

        // Loads registered investigations into the dropdown using their case numbers.
        private async Task LoadInvestigations()
        {
            ViewData["InvestigationId"] = new SelectList(
                await _context.Investigations.ToListAsync(),
                "Id",
                "CaseNumber");
        }
    }
}