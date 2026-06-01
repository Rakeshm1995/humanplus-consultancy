using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Enums;

namespace HumanPlus.Domain.Entities.Jobs
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int JobDemandId { get; set; }
        public JobDemand JobDemand { get; set; } = null!;
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public CandidateStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
