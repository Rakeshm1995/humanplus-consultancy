using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Communication;
using HumanPlus.Domain.Entities.Jobs;

namespace Humanplus_Manpower_Consulting.Models
{
    public class CandidateDashboardViewModel
    {
        public Candidate Candidate { get; set; } = null!;
        public List<JobApplication> RecentApplications { get; set; } = new();
        public List<Interview> UpcomingInterviews { get; set; } = new();
        public Placement? Placement { get; set; }
        public List<Notification> RecentNotifications { get; set; } = new();
        public int UnreadNotificationCount { get; set; }
        public int ProfileCompletionPercent { get; set; }
        public int TotalApplications { get; set; }
        public int TotalInterviews { get; set; }
    }
}
