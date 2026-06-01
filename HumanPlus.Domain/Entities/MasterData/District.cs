using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.MasterData
{
    public class District
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        public int StateId { get; set; }
        public State State { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
