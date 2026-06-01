using System.ComponentModel.DataAnnotations;

namespace HumanPlus.Domain.Entities.Financials
{
    public class Invoice
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public Employers.Employer Employer { get; set; } = null!;
        [Required, MaxLength(50)] public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        [MaxLength(200)] public string? Description { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
