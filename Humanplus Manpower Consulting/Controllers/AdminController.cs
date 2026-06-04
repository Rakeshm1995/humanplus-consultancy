using HumanPlus.Application.Interfaces;
using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Employers;
using HumanPlus.Domain.Entities.Financials;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;
using HumanPlus.Infrastructure.Data;
using Humanplus_Manpower_Consulting.Filters;
using Humanplus_Manpower_Consulting.Models;
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
        private readonly IDocumentService _documentService;
        private readonly INotificationService _notificationService;

        public AdminController(HumanPlusDbContext db, UserManager<ApplicationUser> userManager,
            IDocumentService documentService, INotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _documentService = documentService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.UtcNow;
            var twelveMonthsAgo = now.AddMonths(-12);

            // Basic stats
            var totalJobSeekers = await _db.Candidates.CountAsync();
            var activeJobSeekers = await _db.Candidates.CountAsync(c => c.Status == CandidateStatus.Active);
            var pendingRegistrations = await _db.Candidates.CountAsync(c => c.Status == CandidateStatus.NewRegistration);
            var pendingVerifications = await _db.Candidates.CountAsync(c => c.Status == CandidateStatus.PendingVerification);
            var totalEmployers = await _db.Employers.CountAsync();
            var activeEmployers = await _db.Employers.CountAsync(e => e.Status == EmployerStatus.Active);
            var activeDemands = await _db.JobDemands.CountAsync(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned);
            var pendingDemands = await _db.JobDemands.CountAsync(j => j.Status == JobDemandStatus.PendingApproval);
            var totalPlacements = await _db.Placements.CountAsync();
            var upcomingInterviews = await _db.Interviews.CountAsync(i => i.InterviewDate > now);

            // Revenue summary
            var totalFeesCollected = await _db.FeePayments.Where(f => f.IsVerified).SumAsync(f => (decimal?)f.Amount) ?? 0;
            var pendingFeeCount = await _db.Candidates.CountAsync(c => !c.IsFeePaid && c.IsProfileComplete);
            var totalCommission = await _db.CommissionRecords.SumAsync(c => (decimal?)c.CommissionAmount) ?? 0;

            // Monthly registrations (last 12 months)
            var candidateMonthly = await _db.Candidates
                .Where(c => c.CreatedAt >= twelveMonthsAgo)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var employerMonthly = await _db.Employers
                .Where(e => e.CreatedAt >= twelveMonthsAgo)
                .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var monthlyRegistrations = new List<MonthlyRegistrationData>();
            for (int i = 11; i >= 0; i--)
            {
                var dt = now.AddMonths(-i);
                monthlyRegistrations.Add(new MonthlyRegistrationData
                {
                    Month = dt.ToString("MMM yyyy"),
                    CandidateCount = candidateMonthly.FirstOrDefault(c => c.Year == dt.Year && c.Month == dt.Month)?.Count ?? 0,
                    EmployerCount = employerMonthly.FirstOrDefault(e => e.Year == dt.Year && e.Month == dt.Month)?.Count ?? 0
                });
            }

            // Sector-wise statistics
            var candidateSectors = await _db.Candidates
                .Where(c => c.PreferredIndustryId != null)
                .GroupBy(c => c.PreferredIndustryId!.Value)
                .Select(g => new { IndustryId = g.Key, Count = g.Count() })
                .ToListAsync();

            var employerSectors = await _db.Employers
                .GroupBy(e => e.IndustryId)
                .Select(g => new { IndustryId = g.Key, Count = g.Count() })
                .ToListAsync();

            var industries = await _db.Industries.ToListAsync();

            var sectorStats = industries
                .Select(i => new SectorStatData
                {
                    IndustryName = i.Name,
                    CandidateCount = candidateSectors.FirstOrDefault(c => c.IndustryId == i.Id)?.Count ?? 0,
                    EmployerCount = employerSectors.FirstOrDefault(e => e.IndustryId == i.Id)?.Count ?? 0
                })
                .Where(s => s.CandidateCount > 0 || s.EmployerCount > 0)
                .OrderByDescending(s => s.CandidateCount + s.EmployerCount)
                .ToList();

            // Recent registrations
            var recentCandidates = await _db.Candidates.Include(c => c.User).OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
            var recentEmployers = await _db.Employers.Include(e => e.User).OrderByDescending(e => e.CreatedAt).Take(5).ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalJobSeekers = totalJobSeekers,
                ActiveJobSeekers = activeJobSeekers,
                PendingRegistrations = pendingRegistrations,
                PendingVerifications = pendingVerifications,
                TotalEmployers = totalEmployers,
                ActiveEmployers = activeEmployers,
                ActiveDemands = activeDemands,
                PendingDemands = pendingDemands,
                TotalPlacements = totalPlacements,
                UpcomingInterviews = upcomingInterviews,
                TotalFeesCollected = totalFeesCollected,
                PendingFeeCount = pendingFeeCount,
                TotalCommission = totalCommission,
                MonthlyRegistrations = monthlyRegistrations,
                SectorStats = sectorStats,
                RecentCandidates = recentCandidates,
                RecentEmployers = recentEmployers
            };

            return View(vm);
        }

        public async Task<IActionResult> Candidates(string? status, string? search)
        {
            var query = _db.Candidates
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Include(c => c.Placements)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<CandidateStatus>(status, out var candidateStatus))
                query = query.Where(c => c.Status == candidateStatus);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.User.FullName.Contains(search) || c.User.Email!.Contains(search) || c.AadhaarNumber!.Contains(search));

            ViewBag.CandidateStatuses = Enum.GetValues<CandidateStatus>();
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;
            ViewBag.ActiveDemands = await _db.JobDemands
                .Where(j => j.Status == JobDemandStatus.Approved || j.Status == JobDemandStatus.CandidatesAssigned)
                .Include(j => j.Employer)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("ToggleFeePaid")]
        public async Task<IActionResult> MarkFeePaid(int candidateId)
        {
            var candidate = await _db.Candidates.FindAsync(candidateId);
            if (candidate != null)
            {
                candidate.IsFeePaid = !candidate.IsFeePaid;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                var status = candidate.IsFeePaid ? "paid" : "unpaid";
                TempData["Success"] = $"Registration fee marked as {status} for candidate #{candidateId}";
            }
            return RedirectToAction(nameof(Candidates));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("ToggleOfficeVisited")]
        public async Task<IActionResult> MarkOfficeVisited(int candidateId)
        {
            var candidate = await _db.Candidates.FindAsync(candidateId);
            if (candidate != null)
            {
                candidate.IsOfficeVisited = !candidate.IsOfficeVisited;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                var status = candidate.IsOfficeVisited ? "visited" : "not visited";
                TempData["Success"] = $"Office visit marked as {status} for candidate #{candidateId}";
            }
            return RedirectToAction(nameof(Candidates));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("ApproveCandidate")]
        public async Task<IActionResult> ApproveCandidate(int candidateId)
        {
            var candidate = await _db.Candidates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == candidateId);
            if (candidate != null)
            {
                candidate.Status = CandidateStatus.Active;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                if (candidate.User != null)
                    await _notificationService.SendNotificationAsync(candidate.UserId, "Your registration has been approved. Welcome to HumanPlus!", "Registration");
                TempData["Success"] = $"Candidate #{candidateId} approved successfully";
            }
            return RedirectToAction(nameof(Candidates));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("RejectCandidate")]
        public async Task<IActionResult> RejectCandidate(int candidateId)
        {
            var candidate = await _db.Candidates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == candidateId);
            if (candidate != null)
            {
                candidate.Status = CandidateStatus.Rejected;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                if (candidate.User != null)
                    await _notificationService.SendNotificationAsync(candidate.UserId, "Your registration has been reviewed and was not approved at this time. Please contact support for more information.", "Registration");
                TempData["Success"] = $"Candidate #{candidateId} rejected";
            }
            return RedirectToAction(nameof(Candidates));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("ToggleCandidateActive")]
        public async Task<IActionResult> ToggleCandidateActive(int candidateId)
        {
            var candidate = await _db.Candidates.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == candidateId);
            if (candidate != null)
            {
                candidate.Status = candidate.Status == CandidateStatus.Active ? CandidateStatus.Inactive : CandidateStatus.Active;
                candidate.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Candidate #{candidateId} is now {candidate.Status}";
            }
            return RedirectToAction(nameof(Candidates));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("VerifyDocument")]
        public async Task<IActionResult> VerifyDocument(int documentId)
        {
            var doc = await _db.CandidateDocuments.FindAsync(documentId);
            if (doc != null)
            {
                doc.IsVerified = true;
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Document not found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog("AssignToJob")]
        public async Task<IActionResult> AssignToJob(int candidateId, int jobDemandId)
        {
            var candidate = await _db.Candidates.FindAsync(candidateId);
            var demand = await _db.JobDemands.FindAsync(jobDemandId);
            if (candidate == null || demand == null)
            {
                TempData["Error"] = "Candidate or Job Demand not found";
                return RedirectToAction(nameof(Candidates));
            }

            var existing = await _db.CandidateAssignments
                .FirstOrDefaultAsync(a => a.CandidateId == candidateId && a.JobDemandId == jobDemandId);
            if (existing == null)
            {
                _db.CandidateAssignments.Add(new CandidateAssignment
                {
                    CandidateId = candidateId,
                    JobDemandId = jobDemandId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedByUserId = _userManager.GetUserId(User)
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Candidate assigned to job '{demand.JobTitle}'";
            }
            else
            {
                TempData["Error"] = "Candidate is already assigned to this job";
            }
            return RedirectToAction(nameof(Candidates));
        }

        public async Task<IActionResult> DownloadCandidateProfile(int id)
        {
            var candidate = await _db.Candidates
                .Include(c => c.User)
                .Include(c => c.State)
                .Include(c => c.District)
                .Include(c => c.PreferredIndustry)
                .Include(c => c.PreviousIndustry)
                .Include(c => c.Educations).ThenInclude(e => e.Qualification)
                .Include(c => c.Skills).ThenInclude(s => s.Skill)
                .Include(c => c.Experiences)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidate == null) return NotFound();

            var pdf = await _documentService.GenerateCandidateRegistrationFormAsync(candidate);
            return File(pdf, "application/pdf", $"RegistrationForm_{candidate.User?.FullName?.Replace(" ", "_")}.pdf");
        }

        public async Task<IActionResult> CandidateDocuments(int id)
        {
            var candidate = await _db.Candidates
                .Include(c => c.User)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidate == null) return NotFound();

            return PartialView("_CandidateDocuments", candidate);
        }

        public async Task<IActionResult> CandidatePlacements(int id)
        {
            var candidate = await _db.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (candidate == null) return NotFound();

            var placements = await _db.Placements
                .Where(p => p.CandidateId == id)
                .Include(p => p.JobDemand).ThenInclude(j => j.Employer)
                .OrderByDescending(p => p.PlacementDate)
                .ToListAsync();

            ViewBag.Placements = placements;
            return PartialView("_CandidatePlacements", candidate);
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
