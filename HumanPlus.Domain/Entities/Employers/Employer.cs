using System.ComponentModel.DataAnnotations;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Enums;

namespace HumanPlus.Domain.Entities.Employers
{
    public class Employer
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        public Identity.ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(200)] public string CompanyName { get; set; } = string.Empty;
        [MaxLength(100)] public string? BusinessType { get; set; }
        public int? IndustryId { get; set; }
        public Industry? Industry { get; set; }
        [MaxLength(50)] public string? GstNumber { get; set; }
        [MaxLength(50)] public string? CinNumber { get; set; }
        [MaxLength(200)] public string? Website { get; set; }
        [MaxLength(500)] public string? OfficeAddress { get; set; }
        public int? DistrictId { get; set; }
        public District? District { get; set; }
        public int? StateId { get; set; }
        public State? State { get; set; }
        [MaxLength(10)] public string? PinCode { get; set; }

        [MaxLength(100)] public string? ContactPersonName { get; set; }
        [MaxLength(100)] public string? ContactPersonDesignation { get; set; }
        [MaxLength(20)] public string? ContactPersonMobile { get; set; }
        [MaxLength(100)] public string? ContactPersonEmail { get; set; }

        [MaxLength(500)] public string? ManpowerTypeRequired { get; set; }
        [MaxLength(500)] public string? ServiceLocations { get; set; }
        public int? ApproximateHiringVolume { get; set; }

        public EmployerStatus Status { get; set; } = EmployerStatus.PendingVerification;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<EmployerDocument> Documents { get; set; } = new List<EmployerDocument>();
        public ICollection<EmployerSubscription> Subscriptions { get; set; } = new List<EmployerSubscription>();
    }
}
