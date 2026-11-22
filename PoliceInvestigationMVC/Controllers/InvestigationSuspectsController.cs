using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using PoliceInvestigationMVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace PoliceInvestigationMVC.Controllers
{
    [Authorize(Roles = "Administrator,PoliceOfficer")]
    public class InvestigationSuspectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvestigationSuspectsController(ApplicationDbContext context)
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

        // Displays all suspected persons.
        public async Task<IActionResult> Index(string searchString)
        {
            var suspects = _context.InvestigationSuspects
                .Include(s => s.Investigation)
                .AsQueryable();

            // Administrators can view all suspect records.
            if (User.IsInRole("Administrator"))
            {
                // No additional filtering required.
            }
            else
            {
                // Officers can only view suspects linked to their assigned cases.
                var officer = await GetLoggedInOfficerAsync();

                if (officer == null)
                {
                    return Forbid();
                }

                suspects = suspects.Where(s =>
                    s.Investigation != null &&
                    s.Investigation.InvestigatingOfficerId == officer.Id);
            }

            // Search by suspect name, ID number or investigation case number.
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                suspects = suspects.Where(s =>
                    (s.FullName != null && s.FullName.Contains(searchString)) ||
                    (s.IDNumber != null && s.IDNumber.Contains(searchString)) ||
                    (s.Investigation != null &&
                     s.Investigation.CaseNumber.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await suspects.ToListAsync());
        }

        // Displays suspects linked to a specific investigation.
        public async Task<IActionResult> ByInvestigation(int investigationId)
        {
            var investigation = await _context.Investigations
                .FirstOrDefaultAsync(i => i.Id == investigationId);

            if (investigation == null)
            {
                return NotFound();
            }

            // Officers can only access their assigned investigations.
            if (User.IsInRole("PoliceOfficer"))
            {
                var officer = await GetLoggedInOfficerAsync();

                if (officer == null)
                {
                    return Forbid();
                }

                if (investigation.InvestigatingOfficerId != officer.Id)
                {
                    return Forbid();
                }
            }

            var suspects = await _context.InvestigationSuspects
                .Where(s => s.InvestigationId == investigationId)
                .Include(s => s.Investigation)
                .ToListAsync();

            ViewBag.CaseNumber = investigation.CaseNumber;
            ViewBag.InvestigationId = investigationId;

            return View(suspects);
        }

        // Displays the details of a selected suspect.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suspect = await _context.InvestigationSuspects
                .Include(s => s.Investigation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (suspect == null)
            {
                return NotFound();
            }

            // Officers can only view suspects linked to their assigned cases.
            if (User.IsInRole("PoliceOfficer"))
            {
                var officer = await GetLoggedInOfficerAsync();

                if (officer == null)
                {
                    return Forbid();
                }

                if (suspect.Investigation?.InvestigatingOfficerId != officer.Id)
                {
                    return Forbid();
                }
            }

            return View(suspect);
        }

        // Loads the registration form.
        [Authorize(Roles = "PoliceOfficer")]
        public async Task<IActionResult> Create(int? investigationId)
        {
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            // Load only investigations assigned to this officer.
            var investigations = await _context.Investigations
                .Where(i => i.InvestigatingOfficerId == officer.Id)
                .OrderBy(i => i.CaseNumber)
                .ToListAsync();

            ViewData["InvestigationId"] = new SelectList(
                investigations,
                "Id",
                "CaseNumber",
                investigationId);

            return View();
        }

        // Saves a new suspected person.
        [Authorize(Roles = "PoliceOfficer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InvestigationSuspect suspect,
            IFormFile? idPhoto,
            IFormFile? idCopy)
        {
            // Identify the logged-in Police Officer.
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            // Confirm that the selected investigation belongs to this officer.
            var assignedInvestigation = await _context.Investigations
                .FirstOrDefaultAsync(i =>
                    i.Id == suspect.InvestigationId &&
                    i.InvestigatingOfficerId == officer.Id);

            if (assignedInvestigation == null)
            {
                return Forbid();
            }

            // Allowed file formats.
            var photoExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var documentExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

            const long maxFileSize = 10 * 1024 * 1024;

            // Validate the suspect's photograph.
            if (idPhoto != null && idPhoto.Length > 0)
            {
                var extension = Path.GetExtension(idPhoto.FileName)
                    .ToLowerInvariant();

                if (!photoExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "idPhoto",
                        "The photograph must be JPG, JPEG or PNG.");
                }
                else if (idPhoto.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "idPhoto",
                        "The photograph cannot exceed 10 MB.");
                }
            }

            // Validate the identity document.
            if (idCopy != null && idCopy.Length > 0)
            {
                var extension = Path.GetExtension(idCopy.FileName)
                    .ToLowerInvariant();

                if (!documentExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "idCopy",
                        "The ID copy must be JPG, JPEG, PNG or PDF.");
                }
                else if (idCopy.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "idCopy",
                        "The ID copy cannot exceed 10 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Save the suspect record first.
                _context.InvestigationSuspects.Add(suspect);
                await _context.SaveChangesAsync();

                // Create the private upload folder.
                var uploadFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data",
                    "SuspectDocuments");

                Directory.CreateDirectory(uploadFolder);

                // Save the suspect photograph.
                if (idPhoto != null && idPhoto.Length > 0)
                {
                    var extension = Path.GetExtension(idPhoto.FileName)
                        .ToLowerInvariant();

                    var storedFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await idPhoto.CopyToAsync(stream);
                    }

                    suspect.IDPhotoFileName = Path.GetFileName(idPhoto.FileName);
                    suspect.IDPhotoFilePath = Path.Combine(
                        "App_Data",
                        "SuspectDocuments",
                        storedFileName);
                }

                // Save the identity document copy.
                if (idCopy != null && idCopy.Length > 0)
                {
                    var extension = Path.GetExtension(idCopy.FileName)
                        .ToLowerInvariant();

                    var storedFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await idCopy.CopyToAsync(stream);
                    }

                    suspect.IDCopyFileName = Path.GetFileName(idCopy.FileName);
                    suspect.IDCopyFilePath = Path.Combine(
                        "App_Data",
                        "SuspectDocuments",
                        storedFileName);
                }

                // Save the uploaded file information.
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await PopulateInvestigationsAsync(suspect.InvestigationId);

            return View(suspect);
        }

        // Loads the edit form.
        [Authorize(Roles = "PoliceOfficer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suspect = await _context.InvestigationSuspects
                .Include(s => s.Investigation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (suspect == null)
            {
                return NotFound();
            }

            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            // Confirm that the suspect belongs to an assigned investigation.
            if (suspect.Investigation?.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }

            await PopulateInvestigationsAsync(suspect.InvestigationId);

            return View(suspect);
        }

        // Updates an existing suspected person.
        [Authorize(Roles = "PoliceOfficer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            InvestigationSuspect suspect,
            IFormFile? idPhoto,
            IFormFile? idCopy)
        {
            if (id != suspect.Id)
                return NotFound();

            var existingSuspect = await _context.InvestigationSuspects
    .Include(s => s.Investigation)
    .FirstOrDefaultAsync(s => s.Id == id);

            if (existingSuspect == null)
            {
                return NotFound();
            }

            // Identify the logged-in Police Officer.
            var officer = await GetLoggedInOfficerAsync();

            if (officer == null)
            {
                return Forbid();
            }

            // Confirm that the suspect belongs to an investigation assigned to this officer.
            if (existingSuspect.Investigation?.InvestigatingOfficerId != officer.Id)
            {
                return Forbid();
            }

            // Preserve the original investigation link.
            suspect.InvestigationId = existingSuspect.InvestigationId;
            ModelState.Remove(nameof(InvestigationSuspect.InvestigationId));

            var photoExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var documentExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

            const long maxFileSize = 10 * 1024 * 1024;

            // Validate the replacement photograph.
            if (idPhoto != null && idPhoto.Length > 0)
            {
                var extension = Path.GetExtension(idPhoto.FileName)
                    .ToLowerInvariant();

                if (!photoExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "idPhoto",
                        "The photograph must be JPG, JPEG or PNG.");
                }
                else if (idPhoto.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "idPhoto",
                        "The photograph cannot exceed 10 MB.");
                }
            }

            // Validate the replacement ID copy.
            if (idCopy != null && idCopy.Length > 0)
            {
                var extension = Path.GetExtension(idCopy.FileName)
                    .ToLowerInvariant();

                if (!documentExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "idCopy",
                        "The ID copy must be JPG, JPEG, PNG or PDF.");
                }
                else if (idCopy.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "idCopy",
                        "The ID copy cannot exceed 10 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                // Update the suspect's information.
                existingSuspect.FullName = suspect.FullName;
                existingSuspect.IDNumber = suspect.IDNumber;
                existingSuspect.ContactNumber = suspect.ContactNumber;
                existingSuspect.PhysicalDescription = suspect.PhysicalDescription;
                existingSuspect.IdentificationStatus = suspect.IdentificationStatus;
                existingSuspect.ArrestStatus = suspect.ArrestStatus;
                existingSuspect.IdentificationMethod = suspect.IdentificationMethod;
                existingSuspect.Notes = suspect.Notes;

                var uploadFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data",
                    "SuspectDocuments");

                Directory.CreateDirectory(uploadFolder);

                // Replace the photograph only if a new one was selected.
                if (idPhoto != null && idPhoto.Length > 0)
                {
                    var extension = Path.GetExtension(idPhoto.FileName)
                        .ToLowerInvariant();

                    var storedFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await idPhoto.CopyToAsync(stream);
                    }

                    existingSuspect.IDPhotoFileName =
                        Path.GetFileName(idPhoto.FileName);

                    existingSuspect.IDPhotoFilePath = Path.Combine(
                        "App_Data",
                        "SuspectDocuments",
                        storedFileName);
                }

                // Replace the ID copy only if a new one was selected.
                if (idCopy != null && idCopy.Length > 0)
                {
                    var extension = Path.GetExtension(idCopy.FileName)
                        .ToLowerInvariant();

                    var storedFileName = $"{Guid.NewGuid()}{extension}";
                    var fullPath = Path.Combine(uploadFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await idCopy.CopyToAsync(stream);
                    }

                    existingSuspect.IDCopyFileName =
                        Path.GetFileName(idCopy.FileName);

                    existingSuspect.IDCopyFilePath = Path.Combine(
                        "App_Data",
                        "SuspectDocuments",
                        storedFileName);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Preserve existing document information if validation fails.
            suspect.IDPhotoFileName = existingSuspect.IDPhotoFileName;
            suspect.IDPhotoFilePath = existingSuspect.IDPhotoFilePath;
            suspect.IDCopyFileName = existingSuspect.IDCopyFileName;
            suspect.IDCopyFilePath = existingSuspect.IDCopyFilePath;

            await PopulateInvestigationsAsync(suspect.InvestigationId);

            return View(suspect);
        }

        // Loads the delete confirmation page.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var suspect = await _context.InvestigationSuspects
                .FirstOrDefaultAsync(s => s.Id == id);

            if (suspect == null)
            {
                return NotFound();
            }

            TempData["ErrorMessage"] =
                "Deletion is not permitted. Suspect records must be retained for investigation history and accountability.";

            return RedirectToAction(nameof(Index));
        }

        // Deletes a suspected person.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            TempData["ErrorMessage"] =
                "Deletion is not permitted. Suspect records must be retained for investigation history and accountability.";

            return RedirectToAction(nameof(Index));
        }

        // Populates the investigation dropdown.
        private async Task PopulateInvestigationsAsync(int? selectedId = null)
        {
            if (User.IsInRole("Administrator"))
            {
                var allInvestigations = await _context.Investigations
                    .OrderBy(i => i.CaseNumber)
                    .ToListAsync();

                ViewData["InvestigationId"] = new SelectList(
                    allInvestigations,
                    "Id",
                    "CaseNumber",
                    selectedId);
            }
            else
            {
                var officer = await GetLoggedInOfficerAsync();

                if (officer == null)
                {
                    ViewData["InvestigationId"] = new SelectList(
                        Enumerable.Empty<Investigation>(),
                        "Id",
                        "CaseNumber");

                    return;
                }

                var assignedInvestigations = await _context.Investigations
                    .Where(i => i.InvestigatingOfficerId == officer.Id)
                    .OrderBy(i => i.CaseNumber)
                    .ToListAsync();

                ViewData["InvestigationId"] = new SelectList(
                    assignedInvestigations,
                    "Id",
                    "CaseNumber",
                    selectedId);
            }
        }

        // Downloads the suspect's photograph.
        [HttpGet]
        public async Task<IActionResult> DownloadIDPhoto(int id)
        {
            var suspect = await _context.InvestigationSuspects
                .FirstOrDefaultAsync(s => s.Id == id);

            if (suspect == null || string.IsNullOrEmpty(suspect.IDPhotoFilePath))
                return NotFound();

            var fullPath = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                suspect.IDPhotoFilePath));

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(
                fullPath,
                "application/octet-stream",
                Path.GetFileName(suspect.IDPhotoFileName));
        }

        // Downloads the suspect's identity document copy.
        [HttpGet]
        public async Task<IActionResult> DownloadIDCopy(int id)
        {
            var suspect = await _context.InvestigationSuspects
                .FirstOrDefaultAsync(s => s.Id == id);

            if (suspect == null || string.IsNullOrEmpty(suspect.IDCopyFilePath))
                return NotFound();

            var fullPath = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                suspect.IDCopyFilePath));

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(
                fullPath,
                "application/octet-stream",
                Path.GetFileName(suspect.IDCopyFileName));
        }
    }
}