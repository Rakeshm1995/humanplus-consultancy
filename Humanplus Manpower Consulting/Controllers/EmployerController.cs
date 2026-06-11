using HumanPlus.Domain.Entities.Employers;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;
using HumanPlus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;

        public EmployerController(HumanPlusDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
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
        public async Task<IActionResult> SaveProfile(Employer model,
            IFormFile? GstCertificate, IFormFile? RegistrationCertificate, IFormFile? AddressProof, IFormFile? AuthorizationLetter)
        {
            var user = await _userManager.GetUserAsync(User);
            model.UserId = user!.Id;
            model.User = null!;
            ModelState.Remove("UserId");
            ModelState.Remove("User");

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

            var existing = await _db.Employers
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.UserId == user!.Id);

            int employerId;

            if (existing == null)
            {
                model.Status = EmployerStatus.PendingVerification;
                _db.Employers.Add(model);
                await _db.SaveChangesAsync();
                employerId = model.Id;
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
                employerId = existing.Id;
            }

            await _db.SaveChangesAsync();

            var docs = new[] {
                (file: GstCertificate, type: "GSTCertificate"),
                (file: RegistrationCertificate, type: "RegistrationCertificate"),
                (file: AddressProof, type: "AddressProof"),
                (file: AuthorizationLetter, type: "AuthorizationLetter")
            };

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "employers", employerId.ToString());
            var hasNewDocs = false;

            foreach (var (file, docType) in docs)
            {
                if (file == null || file.Length == 0) continue;
                hasNewDocs = true;

                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{docType}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _db.EmployerDocuments.Add(new EmployerDocument
                {
                    EmployerId = employerId,
                    DocumentType = docType,
                    FilePath = $"/uploads/employers/{employerId}/{fileName}",
                    OriginalFileName = file.FileName
                });
            }

            if (hasNewDocs)
                await _db.SaveChangesAsync();

            TempData["Success"] = "Company profile saved.";
            TempData["ShowSuccessModal"] = true;
            return RedirectToAction(nameof(CompanyProfile));
        }

        [HttpGet]
        public async Task<IActionResult> PostDemand()
        {
            ViewBag.Qualifications = await _db.Qualifications.Where(q => q.IsActive).ToListAsync();
            ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDemand(JobDemand model)
        {
            ViewBag.Qualifications = await _db.Qualifications.Where(q => q.IsActive).ToListAsync();
            ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();

            ModelState.Remove("Employer");

            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction(nameof(CompanyProfile));
            }

            if (string.IsNullOrWhiteSpace(model.JobTitle)) ModelState.AddModelError("JobTitle", "Job Title is required.");
            if (model.IndustryId == null || model.IndustryId <= 0) ModelState.AddModelError("IndustryId", "Sector / Industry is required.");
            if (string.IsNullOrWhiteSpace(model.RequiredSkills)) ModelState.AddModelError("RequiredSkills", "Required Skills is required.");
            if (model.NumberOfOpenings <= 0) ModelState.AddModelError("NumberOfOpenings", "Number of Openings must be greater than 0.");
            if (model.QualificationId == null || model.QualificationId <= 0) ModelState.AddModelError("QualificationId", "Qualification is required.");
            if (model.MinExperience == null || model.MinExperience < 0) ModelState.AddModelError("MinExperience", "Min Experience is required.");
            if (model.MaxExperience == null || model.MaxExperience < 0) ModelState.AddModelError("MaxExperience", "Max Experience is required.");
            if (model.MinExperience != null && model.MaxExperience != null && model.MinExperience > model.MaxExperience)
                ModelState.AddModelError("MaxExperience", "Max Experience must be >= Min Experience.");
            if (model.MinSalary == null || model.MinSalary <= 0) ModelState.AddModelError("MinSalary", "Min Salary must be greater than 0.");
            if (model.MaxSalary == null || model.MaxSalary <= 0) ModelState.AddModelError("MaxSalary", "Max Salary must be greater than 0.");
            if (model.MinSalary != null && model.MaxSalary != null && model.MinSalary > model.MaxSalary)
                ModelState.AddModelError("MaxSalary", "Max Salary must be >= Min Salary.");
            if (string.IsNullOrWhiteSpace(model.DutyHours)) ModelState.AddModelError("DutyHours", "Duty Hours is required.");
            if (string.IsNullOrWhiteSpace(model.WorkLocation)) ModelState.AddModelError("WorkLocation", "Work Location is required.");
            if (string.IsNullOrWhiteSpace(model.AccommodationDetails)) ModelState.AddModelError("AccommodationDetails", "Accommodation Details is required.");
            if (string.IsNullOrWhiteSpace(model.FoodFacility)) ModelState.AddModelError("FoodFacility", "Food Facility is required.");
            if (model.MinAge < 18) ModelState.AddModelError("MinAge", "Min Age must be at least 18.");
            if (model.MaxAge < 18) ModelState.AddModelError("MaxAge", "Max Age must be at least 18.");
            if (model.MinAge > model.MaxAge)
                ModelState.AddModelError("MaxAge", "Max Age must be >= Min Age.");
            if (model.ContractDurationMonths == null || model.ContractDurationMonths <= 0) ModelState.AddModelError("ContractDurationMonths", "Contract Duration must be greater than 0.");
            if (model.InterviewDate == null) ModelState.AddModelError("InterviewDate", "Interview Date is required.");
            else if (model.InterviewDate < DateTime.UtcNow.Date) ModelState.AddModelError("InterviewDate", "Interview Date must be today or later.");

            if (!ModelState.IsValid)
            {
                TempData["ShowValidationModal"] = true;
                return View(model);
            }

            model.EmployerId = employer.Id;
            model.Status = JobDemandStatus.PendingApproval;
            _db.JobDemands.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Job demand posted and pending admin approval.";
            TempData["ShowSuccessModal"] = true;
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

        [HttpGet]
        public async Task<IActionResult> Interviews()
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null) return RedirectToAction(nameof(CompanyProfile));

            var demandIds = await _db.JobDemands
                .Where(j => j.EmployerId == employer.Id)
                .Select(j => j.Id)
                .ToListAsync();

            var interviews = await _db.Interviews
                .Include(i => i.JobDemand)
                .Include(i => i.Candidate).ThenInclude(c => c.User)
                .Where(i => demandIds.Contains(i.JobDemandId))
                .OrderByDescending(i => i.InterviewDate)
                .ToListAsync();

            return View(interviews);
        }

        [HttpGet]
        public async Task<IActionResult> HiringProgress(int? jobDemandId)
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null) return RedirectToAction(nameof(CompanyProfile));

            var demands = await _db.JobDemands
                .Where(j => j.EmployerId == employer.Id)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var demandIds = demands.Select(d => d.Id).ToList();

            ViewBag.ApplicationCounts = await _db.JobApplications
                .Where(a => demandIds.Contains(a.JobDemandId))
                .GroupBy(a => a.JobDemandId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.AssignmentCounts = await _db.CandidateAssignments
                .Where(a => demandIds.Contains(a.JobDemandId))
                .GroupBy(a => a.JobDemandId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.InterviewCounts = await _db.Interviews
                .Where(i => demandIds.Contains(i.JobDemandId))
                .GroupBy(i => i.JobDemandId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.PlacementCounts = await _db.Placements
                .Where(p => demandIds.Contains(p.JobDemandId))
                .GroupBy(p => p.JobDemandId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            return View(demands);
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null) return RedirectToAction(nameof(CompanyProfile));

            var demands = await _db.JobDemands
                .Where(j => j.EmployerId == employer.Id)
                .ToListAsync();

            var totalDemands = demands.Count;
            var fulfilled = demands.Count(j => j.Status == JobDemandStatus.Fulfilled);
            var active = demands.Count(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned);
            var pending = demands.Count(j => j.Status == JobDemandStatus.PendingApproval);

            var demandIds = demands.Select(d => d.Id).ToList();
            var totalPlacements = await _db.Placements.CountAsync(p => demandIds.Contains(p.JobDemandId));
            var totalInterviews = await _db.Interviews.CountAsync(i => demandIds.Contains(i.JobDemandId));
            var totalApplications = await _db.JobApplications.CountAsync(a => demandIds.Contains(a.JobDemandId));

            ViewBag.TotalDemands = totalDemands;
            ViewBag.FulfilledDemands = fulfilled;
            ViewBag.ActiveDemands = active;
            ViewBag.PendingApproval = pending;
            ViewBag.TotalPlacements = totalPlacements;
            ViewBag.TotalInterviews = totalInterviews;
            ViewBag.TotalApplications = totalApplications;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Invoices()
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null) return RedirectToAction(nameof(CompanyProfile));

            var invoices = await _db.Invoices
                .Where(i => i.EmployerId == employer.Id)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(invoices);
        }
    }
}
