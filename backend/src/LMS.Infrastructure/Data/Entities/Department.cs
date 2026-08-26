namespace LMS.Infrastructure.Data.Entities;

/// <summary>
/// Represents a company department. Records are soft-deleted via deleted_at.
/// All CRUD actions are recorded in the Audit Trail (FR-27).
/// </summary>
public sealed class Department
{
    public Guid Id { get; set; }

    /// <summary>Unique department name (case-insensitive uniqueness enforced at DB level).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short unique code, max 10 chars (e.g. "ENG", "HR").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Max concurrent leave approvals allowed within this department (FR-26). Default 2.</summary>
    public int OverlapLimit { get; set; } = 2;

    /// <summary>Valid values: Active | Inactive</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Soft-delete timestamp. Non-null marks department as deactivated (FR-23).</summary>
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}
