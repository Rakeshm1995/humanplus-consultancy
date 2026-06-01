using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Candidates
{
    public class CandidateDocument
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        [Required, MaxLength(50)] public string DocumentType { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string FilePath { get; set; } = string.Empty;
        [MaxLength(200)] public string? OriginalFileName { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; }
    }
}
