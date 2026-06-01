using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Financials
{
    public class Receipt
    {
        public int Id { get; set; }
        [Required, MaxLength(450)] public string UserId { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string ReceiptNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        [MaxLength(200)] public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
