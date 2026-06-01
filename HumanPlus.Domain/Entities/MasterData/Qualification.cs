using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.MasterData
{
    public class Qualification
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
