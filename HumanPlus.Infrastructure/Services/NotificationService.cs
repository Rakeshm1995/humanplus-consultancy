using HumanPlus.Domain.Entities.Communication;
using HumanPlus.Infrastructure.Data;
using HumanPlus.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HumanPlus.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HumanPlusDbContext _db;

        public NotificationService(HumanPlusDbContext db) => _db = db;

        public async Task SendNotificationAsync(string userId, string message, string? type = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false
            });
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var n = await _db.Notifications.FindAsync(notificationId);
            if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int limit = 50)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
