using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.System
{
    public class AuditLog
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string Action { get; set; } = string.Empty;
        [MaxLength(200)] public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
