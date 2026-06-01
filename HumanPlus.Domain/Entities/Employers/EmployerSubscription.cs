namespace HumanPlus.Domain.Entities.Employers
{
    public class EmployerSubscription
    {
        public int Id { get; set; }
        public int EmployerId { get; set; }
        public Employer Employer { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
