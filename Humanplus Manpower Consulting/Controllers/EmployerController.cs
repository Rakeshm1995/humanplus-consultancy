using HumanPlus.Domain.Entities.Employers;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;
using HumanPlus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanplus_Manpower_Consulting.Controllers
{
    [Authorize(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly HumanPlusDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployerController(HumanPlusDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers
                .Include(e => e.Industry)
                .FirstOrDefaultAsync(e => e.UserId == user!.Id);

            if (employer == null)
            {
                employer = new Employer { UserId = user!.Id, Status = EmployerStatus.PendingVerification };
                _db.Employers.Add(employer);
                await _db.SaveChangesAsync();
            }

            var demands = await _db.JobDemands
                .Where(j => j.EmployerId == employer.Id)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var demandIds = demands.Select(d => d.Id).ToList();

            var interviewsCount = await _db.Interviews
                .CountAsync(i => demandIds.Contains(i.JobDemandId) && i.InterviewDate >= DateTime.UtcNow);

            var assignedCandidates = await _db.CandidateAssignments
                .CountAsync(ca => demandIds.Contains(ca.JobDemandId));

            ViewBag.TotalDemands = demands.Count;
            ViewBag.ActiveDemands = demands.Count(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned);
            ViewBag.FulfilledDemands = demands.Count(j => j.Status == JobDemandStatus.Fulfilled);
            ViewBag.PendingApproval = demands.Count(j => j.Status == JobDemandStatus.PendingApproval);
            ViewBag.UpcomingInterviews = interviewsCount;
            ViewBag.AssignedCandidates = assignedCandidates;
            ViewBag.RecentDemands = demands.Take(5).ToList();

            return View(employer);
        }

        [HttpGet]
        public async Task<IActionResult> CompanyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.UserId == user!.Id);

            ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();
            ViewBag.Districts = await _db.Districts.Where(d => d.IsActive).ToListAsync();
            ViewBag.States = await _db.States.Where(s => s.IsActive).ToListAsync();

            return View(employer ?? new Employer { UserId = user!.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(Employer model)
        {
            ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();
            ViewBag.Districts = await _db.Districts.Where(d => d.IsActive).ToListAsync();
            ViewBag.States = await _db.States.Where(s => s.IsActive).ToListAsync();

            if (string.IsNullOrWhiteSpace(model.CompanyName)) ModelState.AddModelError("CompanyName", "Company Name is required.");
            if (string.IsNullOrWhiteSpace(model.BusinessType)) ModelState.AddModelError("BusinessType", "Business Type is required.");
            if (model.IndustryId == null || model.IndustryId <= 0) ModelState.AddModelError("IndustryId", "Industry Type is required.");
            if (string.IsNullOrWhiteSpace(model.GstNumber)) ModelState.AddModelError("GstNumber", "GST Number is required.");
            else if (model.GstNumber.Trim().Length != 15) ModelState.AddModelError("GstNumber", "GST Number must be 15 characters.");
            if (string.IsNullOrWhiteSpace(model.Website)) ModelState.AddModelError("Website", "Company Website is required.");
            if (string.IsNullOrWhiteSpace(model.OfficeAddress)) ModelState.AddModelError("OfficeAddress", "Office Address is required.");
            if (model.StateId == null || model.StateId <= 0) ModelState.AddModelError("StateId", "State is required.");
            if (model.DistrictId == null || model.DistrictId <= 0) ModelState.AddModelError("DistrictId", "District is required.");
            if (string.IsNullOrWhiteSpace(model.PinCode)) ModelState.AddModelError("PinCode", "PIN Code is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.PinCode.Trim(), @"^\d{6}$")) ModelState.AddModelError("PinCode", "PIN Code must be 6 digits.");
            if (string.IsNullOrWhiteSpace(model.ContactPersonName)) ModelState.AddModelError("ContactPersonName", "Contact Person Name is required.");
            if (string.IsNullOrWhiteSpace(model.ContactPersonDesignation)) ModelState.AddModelError("ContactPersonDesignation", "Designation is required.");
            if (string.IsNullOrWhiteSpace(model.ContactPersonMobile)) ModelState.AddModelError("ContactPersonMobile", "Mobile Number is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.ContactPersonMobile.Trim(), @"^\d{10}$")) ModelState.AddModelError("ContactPersonMobile", "Mobile Number must be 10 digits.");
            if (string.IsNullOrWhiteSpace(model.ContactPersonEmail)) ModelState.AddModelError("ContactPersonEmail", "Email ID is required.");
            else if (!model.ContactPersonEmail.Contains('@') || !model.ContactPersonEmail.Contains('.')) ModelState.AddModelError("ContactPersonEmail", "Invalid email address.");
            if (string.IsNullOrWhiteSpace(model.ManpowerTypeRequired)) ModelState.AddModelError("ManpowerTypeRequired", "Type of Manpower Required is required.");
            if (string.IsNullOrWhiteSpace(model.ServiceLocations)) ModelState.AddModelError("ServiceLocations", "Service Locations is required.");
            if (model.ApproximateHiringVolume == null || model.ApproximateHiringVolume <= 0) ModelState.AddModelError("ApproximateHiringVolume", "Approximate Hiring Volume is required.");

            if (!ModelState.IsValid)
            {
                TempData["ShowValidationModal"] = true;
                return View("CompanyProfile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            var existing = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);

            if (existing == null)
            {
                model.UserId = user!.Id;
                model.Status = EmployerStatus.PendingVerification;
                _db.Employers.Add(model);
            }
            else
            {
                existing.CompanyName = model.CompanyName;
                existing.BusinessType = model.BusinessType;
                existing.IndustryId = model.IndustryId;
                existing.GstNumber = model.GstNumber;
                existing.CinNumber = model.CinNumber;
                existing.Website = model.Website;
                existing.OfficeAddress = model.OfficeAddress;
                existing.DistrictId = model.DistrictId;
                existing.StateId = model.StateId;
                existing.PinCode = model.PinCode;
                existing.ContactPersonName = model.ContactPersonName;
                existing.ContactPersonDesignation = model.ContactPersonDesignation;
                existing.ContactPersonMobile = model.ContactPersonMobile;
                existing.ContactPersonEmail = model.ContactPersonEmail;
                existing.ManpowerTypeRequired = model.ManpowerTypeRequired;
                existing.ServiceLocations = model.ServiceLocations;
                existing.ApproximateHiringVolume = model.ApproximateHiringVolume;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Company profile saved.";
            return RedirectToAction(nameof(CompanyProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadEmployerDocument(string documentType, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(CompanyProfile));
            }

            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction(nameof(CompanyProfile));
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "employers", employer.Id.ToString());
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{documentType}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _db.EmployerDocuments.Add(new EmployerDocument
            {
                EmployerId = employer.Id,
                DocumentType = documentType,
                FilePath = $"/uploads/employers/{employer.Id}/{fileName}",
                OriginalFileName = file.FileName
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(CompanyProfile));
        }

        [HttpGet]
        public async Task<IActionResult> PostDemand()
        {
            ViewBag.Qualifications = await _db.Qualifications.Where(q => q.IsActive).ToListAsync();
            ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();
            ViewBag.JobCategories = await _db.JobCategories.Where(j => j.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDemand(JobDemand model)
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction(nameof(CompanyProfile));
            }

            model.EmployerId = employer.Id;
            model.Status = JobDemandStatus.PendingApproval;
            _db.JobDemands.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Job demand posted and pending admin approval.";
            return RedirectToAction(nameof(ManageDemands));
        }

        [HttpGet]
        public async Task<IActionResult> ManageDemands(string? search, string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null) return RedirectToAction(nameof(CompanyProfile));

            var query = _db.JobDemands.Where(j => j.EmployerId == employer.Id);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(j => j.JobTitle.Contains(search));

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<JobDemandStatus>(status, out var statusFilter))
                query = query.Where(j => j.Status == statusFilter);

            var demands = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;
            return View(demands);
        }

        [HttpGet]
        public async Task<IActionResult> CandidateMatches(int jobDemandId)
        {
            var job = await _db.JobDemands.FindAsync(jobDemandId);
            if (job == null) return NotFound();

            var candidates = await _db.Candidates
                .Include(c => c.User)
                .Where(c => c.Status == CandidateStatus.Active)
                .ToListAsync();

            ViewBag.JobDemand = job;
            return View(candidates);
        }
    }
}
