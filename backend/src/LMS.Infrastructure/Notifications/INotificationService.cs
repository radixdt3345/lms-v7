namespace LMS.Infrastructure.Notifications;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid userId, Guid[] notificationIds);
    Task CreateAsync(Guid userId, string title, string message, string type, Guid? relatedEntityId = null, string? relatedEntityType = null);
}
