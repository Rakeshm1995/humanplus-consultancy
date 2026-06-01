namespace HumanPlus.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, string? type = null);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
