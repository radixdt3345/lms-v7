namespace LMS.Infrastructure.AuditLogs;

/// <summary>Read-only projection of an AuditLog entry returned to callers.</summary>
public sealed record AuditLogDto(
    Guid Id,
    Guid? ActorUserId,
    string? ActorName,
    string ActionType,
    string RecordType,
    string RecordId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    DateTime Timestamp
);