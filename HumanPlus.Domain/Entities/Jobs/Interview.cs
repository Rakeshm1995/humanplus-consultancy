using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Jobs
{
    public class Interview
    {
        public int Id { get; set; }
        public int JobDemandId { get; set; }
        public JobDemand JobDemand { get; set; } = null!;
        public int CandidateId { get; set; }
        public Candidates.Candidate Candidate { get; set; } = null!;
        public DateTime InterviewDate { get; set; }
        [MaxLength(500)] public string? LocationOrLink { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        public bool IsSelected { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
