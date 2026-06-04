using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Employers;

namespace Humanplus_Manpower_Consulting.Models
{
    public class AdminDashboardViewModel
    {
        // Job Seeker Stats
        public int TotalJobSeekers { get; set; }
        public int ActiveJobSeekers { get; set; }
        public int PendingRegistrations { get; set; }
        public int PendingVerifications { get; set; }

        // Employer Stats
        public int TotalEmployers { get; set; }
        public int ActiveEmployers { get; set; }

        // Demand & Placement Stats
        public int ActiveDemands { get; set; }
        public int PendingDemands { get; set; }
        public int TotalPlacements { get; set; }
        public int UpcomingInterviews { get; set; }

        // Revenue Summary
        public decimal TotalFeesCollected { get; set; }
        public int PendingFeeCount { get; set; }
        public decimal TotalCommission { get; set; }

        // Monthly Registrations (last 12 months)
        public List<MonthlyRegistrationData> MonthlyRegistrations { get; set; } = new();

        // Sector-wise Statistics
        public List<SectorStatData> SectorStats { get; set; } = new();

        // Recent Registrations
        public List<Candidate> RecentCandidates { get; set; } = new();
        public List<Employer> RecentEmployers { get; set; } = new();
    }

    public class MonthlyRegistrationData
    {
        public string Month { get; set; } = string.Empty;
        public int CandidateCount { get; set; }
        public int EmployerCount { get; set; }
    }

    public class SectorStatData
    {
        public string IndustryName { get; set; } = string.Empty;
        public int CandidateCount { get; set; }
        public int EmployerCount { get; set; }
    }
}
