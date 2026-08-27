namespace LMS.Infrastructure.AuditLogs;

public interface IAuditLogService
{
    Task LogAsync(
        string entityType,
        string entityId,
        string action,
        Guid? actorId = null,
        string? actorEmail = null,
        string? changes = null,
        CancellationToken ct = default
    );

    /// <summary>Search audit logs with optional filters and pagination.</summary>
    Task<(IReadOnlyList<AuditLogDto> Items, int TotalCount)> SearchAsync(
        AuditLogSearchParams p,
        CancellationToken ct = default
    );
}