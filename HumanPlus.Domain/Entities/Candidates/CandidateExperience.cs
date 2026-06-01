using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Candidates
{
    public class CandidateExperience
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        [MaxLength(200)] public string? EmployerName { get; set; }
        [MaxLength(100)] public string? Designation { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public decimal? Salary { get; set; }
        [MaxLength(500)] public string? Responsibilities { get; set; }
    }
}
