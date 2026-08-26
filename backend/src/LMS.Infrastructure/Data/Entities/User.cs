namespace LMS.Infrastructure.Data.Entities;

/// <summary>
/// Represents a system user. Records are never hard-deleted — use deleted_at for soft-delete
/// and anonymise personal fields for GDPR Art.17 compliance.
/// </summary>
public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash. Null for SSO-only accounts that have never set a password.</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Valid values: EMPLOYEE | MANAGER | HR_ADMIN | SUPER_ADMIN</summary>
    public string Role { get; set; } = "EMPLOYEE";

    /// <summary>Valid values: Active | Inactive | Locked</summary>
    public string Status { get; set; } = "Active";

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>Consecutive failed login attempts. Resets to 0 on successful login or unlock.</summary>
    public int FailedAttempts { get; set; }

    /// <summary>Timestamp of account lock event. Null when account is not locked.</summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>
    /// Soft-delete timestamp. Non-null marks this record as deactivated.
    /// Personal fields (Name, Email) must be anonymised at this point for GDPR Art.17.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
