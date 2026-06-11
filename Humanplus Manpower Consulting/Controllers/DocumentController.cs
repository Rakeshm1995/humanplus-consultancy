using HumanPlus.Application.Interfaces;
using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Humanplus_Manpower_Consulting.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly HumanPlusDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DocumentController(IDocumentService documentService, HumanPlusDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _documentService = documentService;
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> DownloadRegistrationForm()
        {
            var user = await _userManager.GetUserAsync(User);
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
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (candidate == null)
            {
                TempData["Error"] = "Complete your profile first.";
                return RedirectToAction("MyProfile", "Candidate");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var pdf = await _documentService.GenerateCandidateRegistrationFormAsync(candidate, baseUrl, _env.WebRootPath);
            return File(pdf, "application/pdf", $"RegistrationForm_{candidate.User?.FullName?.Replace(" ", "_")}.pdf");
        }

        public IActionResult Receipt()
        {
            return View();
        }

        public IActionResult Invoice()
        {
            return View();
        }
    }
}
