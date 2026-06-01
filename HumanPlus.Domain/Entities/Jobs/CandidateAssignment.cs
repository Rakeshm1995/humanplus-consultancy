using HumanPlus.Domain.Entities.Candidates;

namespace HumanPlus.Domain.Entities.Jobs
{
    public class CandidateAssignment
    {
        public int Id { get; set; }
        public int JobDemandId { get; set; }
        public JobDemand JobDemand { get; set; } = null!;
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedByUserId { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? ResponseAt { get; set; }
    }
}
