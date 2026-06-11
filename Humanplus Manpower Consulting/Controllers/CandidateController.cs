using HumanPlus.Application.Interfaces;
using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Communication;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Enums;
using HumanPlus.Infrastructure.Data;
using Humanplus_Manpower_Consulting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanplus_Manpower_Consulting.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    public class CandidateController : Controller
    {
        private readonly HumanPlusDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public CandidateController(HumanPlusDbContext db, UserManager<ApplicationUser> userManager, INotificationService notificationService, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _notificationService = notificationService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates
                .Include(c => c.Educations).ThenInclude(e => e.Qualification)
                .Include(c => c.Skills).ThenInclude(s => s.Skill)
                .Include(c => c.Documents)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (candidate == null)
                return RedirectToAction(nameof(MyProfile));

            var applications = await _db.JobApplications
                .Include(a => a.JobDemand)
                .Where(a => a.CandidateId == candidate.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            var interviews = await _db.Interviews
                .Include(i => i.JobDemand)
                .Where(i => i.CandidateId == candidate.Id)
                .OrderByDescending(i => i.InterviewDate)
                .ToListAsync();

            var placement = await _db.Placements
                .Include(p => p.JobDemand)
                .FirstOrDefaultAsync(p => p.CandidateId == candidate.Id);

            var notifications = await _notificationService.GetUserNotificationsAsync(user!.Id, 5);
            var unreadCount = await _notificationService.GetUnreadCountAsync(user.Id);

            var totalFields = 16;
            var filledFields = 0;
            if (!string.IsNullOrWhiteSpace(candidate.FatherName)) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.MotherName)) filledFields++;
            if (candidate.DateOfBirth != null) filledFields++;
            if (candidate.MaritalStatus != null) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.AadhaarNumber)) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.CurrentAddress)) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.PermanentAddress)) filledFields++;
            if (candidate.StateId != null) filledFields++;
            if (candidate.DistrictId != null) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.PinCode)) filledFields++;
            if (candidate.PreferredIndustryId != null) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.PreferredLocation)) filledFields++;
            if (candidate.ExpectedSalary != null) filledFields++;
            if (candidate.ShiftPreference != null) filledFields++;
            if (candidate.PreferredEmploymentType != null) filledFields++;
            if (!string.IsNullOrWhiteSpace(candidate.LanguagesKnown)) filledFields++;
            var hasSkills = await _db.CandidateSkills.AnyAsync(cs => cs.CandidateId == candidate.Id);
            var hasEducation = await _db.CandidateEducations.AnyAsync(ce => ce.CandidateId == candidate.Id);
            if (hasSkills) filledFields++;
            if (hasEducation) filledFields++;
            totalFields += 2;
            if (!candidate.IsFresher)
            {
                totalFields += 4;
                if (candidate.TotalExperienceYears > 0) filledFields++;
                if (!string.IsNullOrWhiteSpace(candidate.PreviousEmployer)) filledFields++;
                if (!string.IsNullOrWhiteSpace(candidate.PreviousDesignation)) filledFields++;
                if (candidate.PreviousSalary > 0) filledFields++;
            }
            var pct = totalFields > 0 ? (int)((double)filledFields / totalFields * 100) : 0;

            var vm = new CandidateDashboardViewModel
            {
                Candidate = candidate,
                RecentApplications = applications.Take(3).ToList(),
                UpcomingInterviews = interviews.Where(i => i.InterviewDate >= DateTime.UtcNow).Take(3).ToList(),
                Placement = placement,
                RecentNotifications = notifications,
                UnreadNotificationCount = unreadCount,
                ProfileCompletionPercent = pct,
                TotalApplications = applications.Count,
                TotalInterviews = interviews.Count
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates
                .Include(c => c.Educations).ThenInclude(e => e.Qualification)
                .Include(c => c.Experiences)
                .Include(c => c.Skills).ThenInclude(s => s.Skill)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            ViewBag.Skills = await _db.Skills.Where(s => s.IsActive).ToListAsync();
            ViewBag.Qualifications = await _db.Qualifications.Where(q => q.IsActive).ToListAsync();
            ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();
            ViewBag.Districts = await _db.Districts.Where(d => d.IsActive).ToListAsync();
            ViewBag.States = await _db.States.Where(s => s.IsActive).ToListAsync();

            return View(candidate ?? new Candidate { UserId = user!.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(Candidate model, List<int> SelectedSkillIds, List<CandidateEducation> Educations, List<CandidateExperience> Experiences, IFormFile? ProfileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            model.UserId = user!.Id;
            model.User = null!;
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (!string.IsNullOrEmpty(model.AadhaarNumber) && !System.Text.RegularExpressions.Regex.IsMatch(model.AadhaarNumber, @"^\d{12}$"))
                ModelState.AddModelError("AadhaarNumber", "Aadhaar Number must be exactly 12 digits.");
            if (!string.IsNullOrEmpty(model.PanNumber))
            {
                model.PanNumber = model.PanNumber.ToUpperInvariant();
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.PanNumber, @"^[A-Z]{5}\d{4}[A-Z]$"))
                    ModelState.AddModelError("PanNumber", "PAN Number format is invalid (e.g. ABCDE1234F).");
            }
            if (!string.IsNullOrEmpty(model.AlternateMobile) && !System.Text.RegularExpressions.Regex.IsMatch(model.AlternateMobile, @"^\d{10}$"))
                ModelState.AddModelError("AlternateMobile", "Alternate Mobile must be exactly 10 digits.");
            if (!string.IsNullOrEmpty(model.PinCode) && !System.Text.RegularExpressions.Regex.IsMatch(model.PinCode, @"^\d{6}$"))
                ModelState.AddModelError("PinCode", "PIN Code must be exactly 6 digits.");

            if (!ModelState.IsValid)
            {
                ViewBag.Skills = await _db.Skills.Where(s => s.IsActive).ToListAsync();
                ViewBag.Qualifications = await _db.Qualifications.Where(q => q.IsActive).ToListAsync();
                ViewBag.Industries = await _db.Industries.Where(i => i.IsActive).ToListAsync();
                ViewBag.Districts = await _db.Districts.Where(d => d.IsActive).ToListAsync();
                ViewBag.States = await _db.States.Where(s => s.IsActive).ToListAsync();
                TempData["ShowValidationModal"] = true;
                return View("MyProfile", model);
            }

            var existing = await _db.Candidates
                .Include(c => c.Educations)
                .Include(c => c.Experiences)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (existing == null)
            {
                model.Status = CandidateStatus.NewRegistration;
                _db.Candidates.Add(model);
                await _db.SaveChangesAsync();
                existing = model;
                TempData["ShowProfileCreatedModal"] = true;
            }
            else
            {
                existing.FatherName = model.FatherName;
                existing.MotherName = model.MotherName;
                existing.Gender = model.Gender;
                existing.DateOfBirth = model.DateOfBirth;
                existing.MaritalStatus = model.MaritalStatus;
                existing.BloodGroup = model.BloodGroup;
                existing.AadhaarNumber = model.AadhaarNumber;
                existing.PanNumber = model.PanNumber;
                existing.AlternateMobile = model.AlternateMobile;
                existing.CurrentAddress = model.CurrentAddress;
                existing.PermanentAddress = model.PermanentAddress;
                existing.DistrictId = model.DistrictId;
                existing.StateId = model.StateId;
                existing.PinCode = model.PinCode;
                existing.IsFresher = model.IsFresher;
                existing.TotalExperienceYears = model.TotalExperienceYears;
                existing.PreviousEmployer = model.PreviousEmployer;
                existing.PreviousDesignation = model.PreviousDesignation;
                existing.PreviousSalary = model.PreviousSalary;
                existing.PreviousIndustryId = model.PreviousIndustryId;
                existing.PreferredIndustryId = model.PreferredIndustryId;
                existing.PreferredLocation = model.PreferredLocation;
                existing.ExpectedSalary = model.ExpectedSalary;
                existing.WillingToRelocate = model.WillingToRelocate;
                existing.ShiftPreference = model.ShiftPreference;
                existing.PreferredEmploymentType = model.PreferredEmploymentType;
                existing.LanguagesKnown = model.LanguagesKnown;
                existing.UpdatedAt = DateTime.UtcNow;
                TempData["ShowProfileUpdatedModal"] = true;
            }

            await _db.SaveChangesAsync();

            var candidateId = existing?.Id ?? model.Id;

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "profiles", candidateId.ToString());
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(ProfileImage.FileName);
                var fileName = $"photo_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }
                var dbPath = $"/uploads/profiles/{candidateId}/{fileName}";
                if (existing != null) existing.ProfileImagePath = dbPath;
                else model.ProfileImagePath = dbPath;
                await _db.SaveChangesAsync();
            }

            if (SelectedSkillIds != null)
            {
                _db.CandidateSkills.RemoveRange(_db.CandidateSkills.Where(cs => cs.CandidateId == candidateId));
                foreach (var skillId in SelectedSkillIds)
                {
                    _db.CandidateSkills.Add(new CandidateSkill { CandidateId = candidateId, SkillId = skillId });
                }
            }

            if (Educations != null)
            {
                _db.CandidateEducations.RemoveRange(_db.CandidateEducations.Where(e => e.CandidateId == candidateId));
                foreach (var edu in Educations.Where(e => e.QualificationId > 0))
                {
                    edu.CandidateId = candidateId;
                    _db.CandidateEducations.Add(edu);
                }
            }

            if (Experiences != null)
            {
                _db.CandidateExperiences.RemoveRange(_db.CandidateExperiences.Where(e => e.CandidateId == candidateId));
                foreach (var exp in Experiences.Where(e => !string.IsNullOrEmpty(e.EmployerName)))
                {
                    exp.CandidateId = candidateId;
                    _db.CandidateExperiences.Add(exp);
                }
            }

            await _db.SaveChangesAsync();

            var candidate = existing ?? model;
            var hasSkills = await _db.CandidateSkills.AnyAsync(cs => cs.CandidateId == candidate.Id);
            var hasEducation = await _db.CandidateEducations.AnyAsync(ce => ce.CandidateId == candidate.Id);

            candidate.IsProfileComplete =
                !string.IsNullOrWhiteSpace(candidate.FatherName) &&
                !string.IsNullOrWhiteSpace(candidate.MotherName) &&
                candidate.DateOfBirth != null &&
                candidate.MaritalStatus != null &&
                !string.IsNullOrWhiteSpace(candidate.AadhaarNumber) &&
                !string.IsNullOrWhiteSpace(candidate.CurrentAddress) &&
                !string.IsNullOrWhiteSpace(candidate.PermanentAddress) &&
                candidate.StateId != null &&
                candidate.DistrictId != null &&
                !string.IsNullOrWhiteSpace(candidate.PinCode) &&
                candidate.PreferredIndustryId != null &&
                !string.IsNullOrWhiteSpace(candidate.PreferredLocation) &&
                candidate.ExpectedSalary != null &&
                candidate.ShiftPreference != null &&
                candidate.PreferredEmploymentType != null &&
                !string.IsNullOrWhiteSpace(candidate.LanguagesKnown) &&
                hasSkills && hasEducation &&
                (candidate.IsFresher || (
                    candidate.TotalExperienceYears > 0 &&
                    !string.IsNullOrWhiteSpace(candidate.PreviousEmployer) &&
                    !string.IsNullOrWhiteSpace(candidate.PreviousDesignation) &&
                    candidate.PreviousSalary > 0 &&
                    candidate.PreviousIndustryId != null
                ));

            await _db.SaveChangesAsync();

            TempData["Success"] = "Profile saved successfully.";
            return RedirectToAction(nameof(MyProfile));
        }

        [HttpGet]
        public async Task<IActionResult> Documents()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (candidate != null)
                ViewBag.IsDeclarationAccepted = candidate.IsDeclarationAccepted;

            return View(candidate?.Documents ?? new List<CandidateDocument>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDeclaration(bool isAccepted)
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (candidate != null)
            {
                candidate.IsDeclarationAccepted = isAccepted;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Documents saved successfully!";
            }
            return RedirectToAction(nameof(Documents));
        }

        [HttpGet]
        public async Task<IActionResult> JobOpportunities()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);

            var jobs = await _db.JobDemands
                .Include(j => j.Employer)
                .Where(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned)
                .ToListAsync();

            if (candidate != null)
            {
                var appliedJobIds = await _db.JobApplications
                    .Where(a => a.CandidateId == candidate.Id)
                    .Select(a => a.JobDemandId)
                    .ToListAsync();
                ViewBag.AppliedJobIds = appliedJobIds;
            }

            return View(jobs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(string documentType, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Documents));
            }

            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (candidate == null)
            {
                TempData["Error"] = "Please complete your profile first.";
                return RedirectToAction(nameof(Documents));
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "candidates", candidate.Id.ToString());
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{documentType}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _db.CandidateDocuments.Add(new CandidateDocument
            {
                CandidateId = candidate.Id,
                DocumentType = documentType,
                FilePath = $"/uploads/candidates/{candidate.Id}/{fileName}",
                OriginalFileName = file.FileName
            });
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Documents));
        }

        [HttpGet]
        public async Task<IActionResult> ApplicationStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (candidate == null) return RedirectToAction(nameof(MyProfile));

            var applications = await _db.JobApplications
                .Include(a => a.JobDemand)
                    .ThenInclude(jd => jd.Employer)
                .Where(a => a.CandidateId == candidate.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpGet]
        public async Task<IActionResult> InterviewSchedule()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (candidate == null) return RedirectToAction(nameof(MyProfile));

            var interviews = await _db.Interviews
                .Include(i => i.JobDemand)
                .Where(i => i.CandidateId == candidate.Id)
                .OrderByDescending(i => i.InterviewDate)
                .ToListAsync();

            return View(interviews);
        }

        [HttpGet]
        public async Task<IActionResult> PlacementStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (candidate == null) return RedirectToAction(nameof(MyProfile));

            var placement = await _db.Placements
                .Include(p => p.JobDemand)
                .FirstOrDefaultAsync(p => p.CandidateId == candidate.Id);

            return View(placement);
        }

        public async Task<IActionResult> Apply(int jobDemandId)
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (candidate == null) return RedirectToAction(nameof(MyProfile));

            var exists = await _db.JobApplications.AnyAsync(a => a.JobDemandId == jobDemandId && a.CandidateId == candidate.Id);
            if (!exists)
            {
                _db.JobApplications.Add(new JobApplication
                {
                    JobDemandId = jobDemandId,
                    CandidateId = candidate.Id,
                    Status = CandidateStatus.NewRegistration
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(JobOpportunities));
        }
    }
}
