using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Communication
{
    public class SmsLog
    {
        public int Id { get; set; }
        [MaxLength(450)] public string? UserId { get; set; }
        [Required, MaxLength(20)] public string MobileNumber { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
