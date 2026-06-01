using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Communication
{
    public class Notification
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string Message { get; set; } = string.Empty;
        [MaxLength(100)] public string? Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
