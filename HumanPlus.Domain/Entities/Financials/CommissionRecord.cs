using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Financials
{
    public class CommissionRecord
    {
        public int Id { get; set; }
        public int PlacementId { get; set; }
        public decimal CommissionAmount { get; set; }
        [MaxLength(50)] public string CommissionType { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
