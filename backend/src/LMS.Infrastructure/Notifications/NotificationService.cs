using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly LmsDbContext _db;
    public NotificationService(LmsDbContext db) => _db = db;

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var q = _db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        return await q.OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.RelatedEntityId, n.RelatedEntityType, n.CreatedAt))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAsReadAsync(Guid userId, Guid[] notificationIds)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && notificationIds.Contains(n.Id))
            .ToListAsync();
        foreach (var n in notifications) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task CreateAsync(Guid userId, string title, string message, string type, Guid? relatedEntityId = null, string? relatedEntityType = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId, Title = title, Message = message, Type = type,
            RelatedEntityId = relatedEntityId, RelatedEntityType = relatedEntityType,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
