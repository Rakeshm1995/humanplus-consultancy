using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Communication
{
    public class EmailLog
    {
        public int Id { get; set; }
        [MaxLength(450)] public string? UserId { get; set; }
        [Required, MaxLength(200)] public string ToEmail { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
