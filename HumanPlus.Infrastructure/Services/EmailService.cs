using HumanPlus.Domain.Entities.Communication;
using HumanPlus.Infrastructure.Data;
using HumanPlus.Application.Interfaces;

namespace HumanPlus.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly HumanPlusDbContext _db;

        public EmailService(HumanPlusDbContext db) => _db = db;

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _db.EmailLogs.Add(new EmailLog
            {
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
                IsSent = true
            });
            await _db.SaveChangesAsync();
        }
    }
}
