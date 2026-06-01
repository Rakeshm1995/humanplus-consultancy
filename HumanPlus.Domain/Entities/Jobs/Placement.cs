using System.ComponentModel.DataAnnotations;
using HumanPlus.Domain.Entities.Candidates;

namespace HumanPlus.Domain.Entities.Jobs
{
    public class Placement
    {
        public int Id { get; set; }
        public int JobDemandId { get; set; }
        public JobDemand JobDemand { get; set; } = null!;
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public DateTime PlacementDate { get; set; }
        public decimal? SalaryOffered { get; set; }
        public DateTime? JoiningDate { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
