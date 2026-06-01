using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Employers;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;
using HumanPlus.Infrastructure.Data;
using Humanplus_Manpower_Consulting.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanplus_Manpower_Consulting.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HumanPlusDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(HumanPlusDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalCandidates = await _db.Candidates.CountAsync();
            ViewBag.ActiveCandidates = await _db.Candidates.CountAsync(c => c.Status == CandidateStatus.Active);
            ViewBag.TotalEmployers = await _db.Employers.CountAsync();
            ViewBag.ActiveEmployers = await _db.Employers.CountAsync(e => e.Status == EmployerStatus.Active);
            ViewBag.PendingCandidates = await _db.Candidates.CountAsync(c => c.Status == CandidateStatus.PendingVerification);
            ViewBag.ActiveDemands = await _db.JobDemands.CountAsync(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned);
            ViewBag.PendingDemands = await _db.JobDemands.CountAsync(j => j.Status == JobDemandStatus.PendingApproval);
            ViewBag.TotalPlacements = await _db.Placements.CountAsync();
            ViewBag.UpcomingInterviews = await _db.Interviews.CountAsync(i => i.InterviewDate > DateTime.UtcNow);
            return View();
        }

        public async Task<IActionResult> Candidates(string? status, string? search)
        {
            var query = _db.Candidates
                .Include(c => c.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<CandidateStatus>(status, out var candidateStatus))
                query = query.Where(c => c.Status == candidateStatus);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.User.FullName.Contains(search) || c.User.Email!.Contains(search) || c.AadhaarNumber!.Contains(search));

            ViewBag.CandidateStatuses = Enum.GetValues<CandidateStatus>();
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("UpdateCandidateStatus")]
        public async Task<IActionResult> UpdateCandidateStatus(int candidateId, CandidateStatus newStatus)
        {
            var candidate = await _db.Candidates.FindAsync(candidateId);
            if (candidate != null)
            {
                candidate.Status = newStatus;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Candidate status updated to {newStatus}";
            }
            return RedirectToAction(nameof(Candidates));
        }

        public async Task<IActionResult> Employers(string? status, string? search)
        {
            var query = _db.Employers
                .Include(e => e.User)
                .Include(e => e.Industry)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<EmployerStatus>(status, out var employerStatus))
                query = query.Where(e => e.Status == employerStatus);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.CompanyName.Contains(search) || e.User.Email!.Contains(search));

            ViewBag.EmployerStatuses = Enum.GetValues<EmployerStatus>();
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(await query.OrderByDescending(e => e.CreatedAt).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("UpdateEmployerStatus")]
        public async Task<IActionResult> UpdateEmployerStatus(int employerId, EmployerStatus newStatus)
        {
            var employer = await _db.Employers.FindAsync(employerId);
            if (employer != null)
            {
                employer.Status = newStatus;
                employer.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Employer status updated to {newStatus}";
            }
            return RedirectToAction(nameof(Employers));
        }

        public async Task<IActionResult> JobDemands(string? status)
        {
            var query = _db.JobDemands
                .Include(j => j.Employer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<JobDemandStatus>(status, out var demandStatus))
                query = query.Where(j => j.Status == demandStatus);

            ViewBag.JobDemandStatuses = Enum.GetValues<JobDemandStatus>();
            ViewBag.SelectedStatus = status;

            return View(await query.OrderByDescending(j => j.CreatedAt).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("UpdateDemandStatus")]
        public async Task<IActionResult> UpdateDemandStatus(int jobDemandId, JobDemandStatus newStatus)
        {
            var demand = await _db.JobDemands.FindAsync(jobDemandId);
            if (demand != null)
            {
                demand.Status = newStatus;
                demand.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Demand status updated to {newStatus}";
            }
            return RedirectToAction(nameof(JobDemands));
        }

        public IActionResult Reports() => View();

        public async Task<IActionResult> MasterData()
        {
            ViewBag.Skills = await _db.Skills.ToListAsync();
            ViewBag.Industries = await _db.Industries.ToListAsync();
            ViewBag.Qualifications = await _db.Qualifications.ToListAsync();
            ViewBag.JobCategories = await _db.JobCategories.ToListAsync();
            ViewBag.States = await _db.States.Include(s => s.Districts).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _db.Skills.Add(new Skill { Name = name });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MasterData));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIndustry(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _db.Industries.Add(new Industry { Name = name });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MasterData));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQualification(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _db.Qualifications.Add(new Qualification { Name = name });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MasterData));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddState(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _db.States.Add(new State { Name = name });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MasterData));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDistrict(int stateId, string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && stateId > 0)
            {
                _db.Districts.Add(new District { Name = name, StateId = stateId });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MasterData));
        }

        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _db.AuditLogs.OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync();
            return View(logs);
        }
    }
}
