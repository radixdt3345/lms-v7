namespace LMS.Infrastructure.Data.Entities;

public sealed class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string? Changes { get; set; }
    public DateTime CreatedAt { get; set; }
}
