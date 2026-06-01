using System.ComponentModel.DataAnnotations;
using HumanPlus.Domain.Entities.MasterData;

namespace HumanPlus.Domain.Entities.Candidates
{
    public class CandidateEducation
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public int QualificationId { get; set; }
        public Qualification Qualification { get; set; } = null!;
        [MaxLength(200)] public string? BoardOrUniversity { get; set; }
        public int? PassingYear { get; set; }
        [MaxLength(10)] public string? PercentageOrGrade { get; set; }
    }
}
