using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<(IReadOnlyList<AuditLogDto> Items, int TotalCount)> SearchAsync(
        AuditLogSearchParams p,
        CancellationToken ct = default
    )
    {
        var query = _db.AuditLogs.AsQueryable();

        if (p.UserId.HasValue)
            query = query.Where(a => a.ActorId == p.UserId);

        if (!string.IsNullOrWhiteSpace(p.ActionType))
            query = query.Where(a => a.Action == p.ActionType);

        if (!string.IsNullOrWhiteSpace(p.RecordType))
            query = query.Where(a => a.EntityType == p.RecordType);

        if (p.FromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= p.FromDate.Value);

        if (p.ToDate.HasValue)
            query = query.Where(a => a.CreatedAt <= p.ToDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.ActorId,
                a.ActorEmail,
                a.Action,
                a.EntityType,
                a.EntityId,
                null,
                a.Changes,
                null,
                a.CreatedAt
            ))
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
