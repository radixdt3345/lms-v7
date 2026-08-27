namespace LMS.Infrastructure.Data.Entities;

/// <summary>
/// Represents a leave type available in the organisation (e.g. Casual, Sick, Earned).
/// Leave types are managed by HR Admin and cannot be hard-deleted.
/// </summary>
public sealed class LeaveType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AnnualDays { get; set; }
    public bool RequiresAttachment { get; set; }
    public bool RequiresHrApproval { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
