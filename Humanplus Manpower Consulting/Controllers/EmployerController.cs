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
                return RedirectToAction(nameof(CompanyProfile));

            var activeDemands = await _db.JobDemands
                .Where(j => j.EmployerId == employer.Id)
                .ToListAsync();

            ViewBag.TotalDemands = activeDemands.Count;
            ViewBag.ActiveDemands = activeDemands.Count(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned);
            ViewBag.FulfilledDemands = activeDemands.Count(j => j.Status == JobDemandStatus.Fulfilled);

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
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Company profile saved.";
            return RedirectToAction(nameof(CompanyProfile));
        }

        [HttpGet]
        public async Task<IActionResult> PostDemand()
        {
            ViewBag.Qualifications = await _db.Qualifications.Where(q => q.IsActive).ToListAsync();
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
        public async Task<IActionResult> ManageDemands()
        {
            var user = await _userManager.GetUserAsync(User);
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.UserId == user!.Id);
            if (employer == null) return RedirectToAction(nameof(CompanyProfile));

            var demands = await _db.JobDemands
                .Where(j => j.EmployerId == employer.Id)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
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
