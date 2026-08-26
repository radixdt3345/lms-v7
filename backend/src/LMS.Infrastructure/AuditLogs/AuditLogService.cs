using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;

namespace LMS.Infrastructure.AuditLogs;

public sealed class AuditLogService : IAuditLogService
{
    private readonly LmsDbContext _db;

    public AuditLogService(LmsDbContext db) => _db = db;

    public async Task LogAsync(
        string entityType,
        string entityId,
        string action,
        Guid? actorId = null,
        string? actorEmail = null,
        string? changes = null,
        CancellationToken ct = default
    )
    {
        _db.AuditLogs.Add(new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ActorId = actorId,
            ActorEmail = actorEmail,
            Changes = changes,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }
}
