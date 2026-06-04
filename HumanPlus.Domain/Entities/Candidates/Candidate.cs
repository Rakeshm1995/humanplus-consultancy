using System.ComponentModel.DataAnnotations;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;

namespace HumanPlus.Domain.Entities.Candidates
{
    public class Candidate
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        public Identity.ApplicationUser User { get; set; } = null!;

        [MaxLength(100)] public string? FatherName { get; set; }
        [MaxLength(100)] public string? MotherName { get; set; }
        public Gender Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        [MaxLength(10)] public string? BloodGroup { get; set; }
        [MaxLength(20)] public string? AadhaarNumber { get; set; }
        [MaxLength(20)] public string? PanNumber { get; set; }
        [MaxLength(20)] public string? AlternateMobile { get; set; }

        [MaxLength(500)] public string? CurrentAddress { get; set; }
        [MaxLength(500)] public string? PermanentAddress { get; set; }
        public int? DistrictId { get; set; }
        public District? District { get; set; }
        public int? StateId { get; set; }
        public State? State { get; set; }
        [MaxLength(10)] public string? PinCode { get; set; }

        [MaxLength(500)] public string? ProfileImagePath { get; set; }
        [MaxLength(500)] public string? LanguagesKnown { get; set; }
        public bool IsFresher { get; set; } = true;
        public int? TotalExperienceYears { get; set; }
        [MaxLength(200)] public string? PreviousEmployer { get; set; }
        [MaxLength(100)] public string? PreviousDesignation { get; set; }
        public decimal? PreviousSalary { get; set; }
        public int? PreviousIndustryId { get; set; }
        public Industry? PreviousIndustry { get; set; }

        public int? PreferredIndustryId { get; set; }
        public Industry? PreferredIndustry { get; set; }
        [MaxLength(200)] public string? PreferredLocation { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public bool? WillingToRelocate { get; set; }
        public ShiftPreference? ShiftPreference { get; set; }
        public EmploymentType? PreferredEmploymentType { get; set; }

        public CandidateStatus Status { get; set; } = CandidateStatus.NewRegistration;
        public bool IsProfileComplete { get; set; }
        public bool IsFeePaid { get; set; }
        public bool IsOfficeVisited { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CandidateEducation> Educations { get; set; } = new List<CandidateEducation>();
        public ICollection<CandidateExperience> Experiences { get; set; } = new List<CandidateExperience>();
        public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
        public ICollection<CandidateDocument> Documents { get; set; } = new List<CandidateDocument>();
        public ICollection<Placement> Placements { get; set; } = new List<Placement>();
    }
}
