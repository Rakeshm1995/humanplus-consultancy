using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Enums;
using HumanPlus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
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

        public CandidateController(HumanPlusDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var candidate = await _db.Candidates
                .Include(c => c.Educations).ThenInclude(e => e.Qualification)
                .Include(c => c.Skills).ThenInclude(s => s.Skill)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (candidate == null)
                return RedirectToAction(nameof(MyProfile));

            return View(candidate);
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
        public async Task<IActionResult> SaveProfile(Candidate model, List<int> SelectedSkillIds, List<CandidateEducation> Educations, List<CandidateExperience> Experiences)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _db.Candidates
                .Include(c => c.Educations)
                .Include(c => c.Experiences)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (existing == null)
            {
                model.UserId = user!.Id;
                model.Status = CandidateStatus.NewRegistration;
                _db.Candidates.Add(model);
                await _db.SaveChangesAsync();
                existing = model;
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
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            var candidateId = existing?.Id ?? model.Id;

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
            return View(candidate?.Documents ?? new List<CandidateDocument>());
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

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "candidates", candidate.Id.ToString());
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

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Documents));
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
