using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Jobs
{
    public class RecruiterAssignment
    {
        public int Id { get; set; }
        public int JobDemandId { get; set; }
        public JobDemand JobDemand { get; set; } = null!;
        [Required, MaxLength(450)] public string RecruiterUserId { get; set; } = string.Empty;
        public Identity.ApplicationUser RecruiterUser { get; set; } = null!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(500)] public string? Notes { get; set; }
    }
}
