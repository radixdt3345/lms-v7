namespace LMS.Infrastructure.Data.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // LeaveApproved | LeaveRejected | CompOffApproved | CompOffRejected | General
    public bool IsRead { get; set; } = false;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; } = null!;
}
