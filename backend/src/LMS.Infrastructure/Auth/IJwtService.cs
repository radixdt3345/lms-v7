using LMS.Infrastructure.Data.Entities;

namespace LMS.Infrastructure.Auth;

public interface IJwtService
{
    /// <summary>Issues a signed RS256 access JWT for the given user. Expires in 24 hours.</summary>
    Task<string> GenerateAccessTokenAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Generates a cryptographically random refresh token (raw bytes, base64url-encoded).
    /// The caller is responsible for storing only the SHA-256 hash.
    /// </summary>
    string GenerateRawRefreshToken();

    /// <summary>
    /// Ensures an active RS256 key pair exists in the database.
    /// If none is present, generates and persists a new 2048-bit RSA key pair.
    /// Called during application startup.
    /// </summary>
    Task EnsureActiveKeyAsync(CancellationToken ct = default);
}
