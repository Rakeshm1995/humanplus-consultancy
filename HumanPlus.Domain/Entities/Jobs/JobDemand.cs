using System.ComponentModel.DataAnnotations;
using HumanPlus.Domain.Entities.Employers;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;

namespace HumanPlus.Domain.Entities.Jobs
{
    public class JobDemand
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public Employer Employer { get; set; } = null!;

        [Required, MaxLength(200)] public string JobTitle { get; set; } = string.Empty;
        public int? IndustryId { get; set; }
        public Industry? Industry { get; set; }
        public int? JobCategoryId { get; set; }
        public JobCategory? JobCategory { get; set; }
        public int? QualificationId { get; set; }
        public Qualification? Qualification { get; set; }
        public int NumberOfOpenings { get; set; }
        public int? MinExperience { get; set; }
        public int? MaxExperience { get; set; }
        [MaxLength(500)] public string? RequiredSkills { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        [MaxLength(100)] public string? DutyHours { get; set; }
        [MaxLength(500)] public string? AccommodationDetails { get; set; }
        [MaxLength(500)] public string? FoodFacility { get; set; }
        [MaxLength(200)] public string? WorkLocation { get; set; }
        public Gender? GenderPreference { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public bool ImmediateJoining { get; set; }
        public int? ContractDurationMonths { get; set; }
        public DateTime? InterviewDate { get; set; }

        public JobDemandStatus Status { get; set; } = JobDemandStatus.PendingApproval;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public ICollection<CandidateAssignment> Assignments { get; set; } = new List<CandidateAssignment>();
        public ICollection<RecruiterAssignment> RecruiterAssignments { get; set; } = new List<RecruiterAssignment>();
    }
}

