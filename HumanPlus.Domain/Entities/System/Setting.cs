using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.System
{
    public class Setting
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Key { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string Value { get; set; } = string.Empty;
        [MaxLength(200)] public string? Description { get; set; }
    }
}
