namespace LMS.Infrastructure.Notifications;

public sealed record NotificationDto(
    Guid Id, string Title, string Message, string Type,
    bool IsRead, Guid? RelatedEntityId, string? RelatedEntityType, DateTime CreatedAt);

public sealed record MarkReadRequest(Guid[] NotificationIds);
