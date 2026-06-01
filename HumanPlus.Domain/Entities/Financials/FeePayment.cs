using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Financials
{
    public class FeePayment
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        [MaxLength(50)] public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        [MaxLength(50)] public string? PaymentMode { get; set; }
        [MaxLength(200)] public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsVerified { get; set; }
        [MaxLength(450)] public string? VerifiedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
