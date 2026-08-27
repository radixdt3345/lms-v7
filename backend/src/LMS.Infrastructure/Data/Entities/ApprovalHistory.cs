namespace LMS.Infrastructure.Data.Entities;

public sealed class ApprovalHistory
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty; // LeaveApplication | CompOffRequest
    public Guid EntityId { get; set; }
    public Guid ActorId { get; set; }
    public string Action { get; set; } = string.Empty; // Approved | Rejected | Cancelled
    public string? Comments { get; set; }
    public DateTime ActedAt { get; set; }
    public User Actor { get; set; } = null!;
}
