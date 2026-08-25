namespace LMS.Infrastructure.Data.Entities;

/// <summary>
/// Persisted refresh token associated with a user session (HttpOnly cookie, 7-day expiry).
/// Deleted on logout (FR-5). Not soft-deleted — explicit delete is correct behaviour here.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token value. The raw token is never stored.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token is revoked before natural expiry.</summary>
    public DateTime? RevokedAt { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
