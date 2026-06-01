using HumanPlus.Domain.Entities.Communication;
using HumanPlus.Infrastructure.Data;
using HumanPlus.Application.Interfaces;

namespace HumanPlus.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly HumanPlusDbContext _db;

        public SmsService(HumanPlusDbContext db) => _db = db;

        public async Task SendSmsAsync(string mobileNumber, string message)
        {
            _db.SmsLogs.Add(new SmsLog
            {
                MobileNumber = mobileNumber,
                Message = message,
                IsSent = true
            });
            await _db.SaveChangesAsync();
        }
    }
}
