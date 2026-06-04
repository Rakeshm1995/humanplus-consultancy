using HumanPlus.Domain.Entities.Candidates;
using HumanPlus.Domain.Entities.Communication;
using HumanPlus.Domain.Entities.Employers;
using HumanPlus.Domain.Entities.Financials;
using HumanPlus.Domain.Entities.Identity;
using HumanPlus.Domain.Entities.Jobs;
using HumanPlus.Domain.Entities.MasterData;
using HumanPlus.Domain.Entities.System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HumanPlus.Infrastructure.Data
{
    public class HumanPlusDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public HumanPlusDbContext(DbContextOptions<HumanPlusDbContext> options) : base(options) { }

        public DbSet<Candidate> Candidates => Set<Candidate>();
        public DbSet<CandidateEducation> CandidateEducations => Set<CandidateEducation>();
        public DbSet<CandidateExperience> CandidateExperiences => Set<CandidateExperience>();
        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
        public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
        public DbSet<Employer> Employers => Set<Employer>();
        public DbSet<EmployerDocument> EmployerDocuments => Set<EmployerDocument>();
        public DbSet<EmployerSubscription> EmployerSubscriptions => Set<EmployerSubscription>();
        public DbSet<JobDemand> JobDemands => Set<JobDemand>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<CandidateAssignment> CandidateAssignments => Set<CandidateAssignment>();
        public DbSet<Interview> Interviews => Set<Interview>();
        public DbSet<Placement> Placements => Set<Placement>();
        public DbSet<FeePayment> FeePayments => Set<FeePayment>();
        public DbSet<CommissionRecord> CommissionRecords => Set<CommissionRecord>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Receipt> Receipts => Set<Receipt>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
        public DbSet<SmsLog> SmsLogs => Set<SmsLog>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<Industry> Industries => Set<Industry>();
        public DbSet<State> States => Set<State>();
        public DbSet<District> Districts => Set<District>();
        public DbSet<Qualification> Qualifications => Set<Qualification>();
        public DbSet<JobCategory> JobCategories => Set<JobCategory>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }

            builder.Entity<Candidate>(e =>
            {
                e.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(c => c.District)
                    .WithMany()
                    .HasForeignKey(c => c.DistrictId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(c => c.State)
                    .WithMany()
                    .HasForeignKey(c => c.StateId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(c => c.PreviousIndustry)
                    .WithMany()
                    .HasForeignKey(c => c.PreviousIndustryId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(c => c.PreferredIndustry)
                    .WithMany()
                    .HasForeignKey(c => c.PreferredIndustryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CandidateEducation>(e =>
            {
                e.HasOne(ce => ce.Candidate)
                    .WithMany(c => c.Educations)
                    .HasForeignKey(ce => ce.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(ce => ce.Qualification)
                    .WithMany()
                    .HasForeignKey(ce => ce.QualificationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CandidateExperience>(e =>
            {
                e.HasOne(ce => ce.Candidate)
                    .WithMany(c => c.Experiences)
                    .HasForeignKey(ce => ce.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CandidateSkill>(e =>
            {
                e.HasOne(cs => cs.Candidate)
                    .WithMany(c => c.Skills)
                    .HasForeignKey(cs => cs.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(cs => cs.Skill)
                    .WithMany()
                    .HasForeignKey(cs => cs.SkillId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CandidateDocument>(e =>
            {
                e.HasOne(cd => cd.Candidate)
                    .WithMany(c => c.Documents)
                    .HasForeignKey(cd => cd.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Employer>(e =>
            {
                e.HasOne(em => em.User)
                    .WithMany()
                    .HasForeignKey(em => em.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(em => em.Industry)
                    .WithMany()
                    .HasForeignKey(em => em.IndustryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(em => em.District)
                    .WithMany()
                    .HasForeignKey(em => em.DistrictId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(em => em.State)
                    .WithMany()
                    .HasForeignKey(em => em.StateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<EmployerDocument>(e =>
            {
                e.HasOne(ed => ed.Employer)
                    .WithMany(em => em.Documents)
                    .HasForeignKey(ed => ed.EmployerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<EmployerSubscription>(e =>
            {
                e.HasOne(es => es.Employer)
                    .WithMany(em => em.Subscriptions)
                    .HasForeignKey(es => es.EmployerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<JobDemand>(e =>
            {
                e.HasOne(jd => jd.Employer)
                    .WithMany()
                    .HasForeignKey(jd => jd.EmployerId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(jd => jd.Industry)
                    .WithMany()
                    .HasForeignKey(jd => jd.IndustryId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(jd => jd.JobCategory)
                    .WithMany()
                    .HasForeignKey(jd => jd.JobCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(jd => jd.Qualification)
                    .WithMany()
                    .HasForeignKey(jd => jd.QualificationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<JobApplication>(e =>
            {
                e.HasOne(ja => ja.JobDemand)
                    .WithMany(jd => jd.Applications)
                    .HasForeignKey(ja => ja.JobDemandId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(ja => ja.Candidate)
                    .WithMany()
                    .HasForeignKey(ja => ja.CandidateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CandidateAssignment>(e =>
            {
                e.HasOne(ca => ca.JobDemand)
                    .WithMany(jd => jd.Assignments)
                    .HasForeignKey(ca => ca.JobDemandId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(ca => ca.Candidate)
                    .WithMany()
                    .HasForeignKey(ca => ca.CandidateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Interview>(e =>
            {
                e.HasOne(i => i.JobDemand)
                    .WithMany()
                    .HasForeignKey(i => i.JobDemandId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(i => i.Candidate)
                    .WithMany()
                    .HasForeignKey(i => i.CandidateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Placement>(e =>
            {
                e.HasOne(p => p.JobDemand)
                    .WithMany()
                    .HasForeignKey(p => p.JobDemandId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Candidate)
                    .WithMany(c => c.Placements)
                    .HasForeignKey(p => p.CandidateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Invoice>(e =>
            {
                e.HasOne(i => i.Employer)
                    .WithMany()
                    .HasForeignKey(i => i.EmployerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<District>(e =>
            {
                e.HasOne(d => d.State)
                    .WithMany(s => s.Districts)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
