using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.System
{
    public class LoginHistory
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        [MaxLength(50)] public string? IpAddress { get; set; }
        [MaxLength(500)] public string? DeviceInfo { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
