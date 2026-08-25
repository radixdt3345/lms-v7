using FluentAssertions;
using LMS.Infrastructure.Data.Entities;
using Xunit;

namespace LMS.UnitTests.Data.Entities;

/// <summary>
/// Unit tests for the RefreshToken entity class.
/// Verifies default values and nullability reflecting DB schema constraints.
/// </summary>
public sealed class RefreshTokenEntityConfigurationTests
{
    [Fact]
    public void RefreshToken_Id_IsGuid()
    {
        var token = new RefreshToken();
        token.Id.Should().BeOfType<Guid>();
    }

    [Fact]
    public void RefreshToken_UserId_IsGuid()
    {
        // FK to users.id — must be a non-nullable Guid (DB enforces FK constraint)
        var token = new RefreshToken();
        token.UserId.Should().BeOfType<Guid>();
    }

    [Fact]
    public void RefreshToken_TokenHash_DefaultsToEmptyString()
    {
        // token_hash is NOT NULL in the DB — raw token is never stored, only its SHA-256 hash
        var token = new RefreshToken();
        token.TokenHash.Should().Be(string.Empty);
    }

    [Fact]
    public void RefreshToken_RevokedAt_IsNullable()
    {
        // revoked_at is NULL when the token is still active; set when explicitly revoked
        var token = new RefreshToken();
        token.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void RefreshToken_IpAddress_DefaultsToEmptyString()
    {
        // ip_address is NOT NULL in the DB
        var token = new RefreshToken();
        token.IpAddress.Should().Be(string.Empty);
    }

    [Fact]
    public void RefreshToken_CanSetRevokedAt()
    {
        var revokedAt = DateTime.UtcNow;
        var token = new RefreshToken { RevokedAt = revokedAt };
        token.RevokedAt.Should().Be(revokedAt);
    }

    [Fact]
    public void RefreshToken_ExpiresAt_IsAssignable()
    {
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = new RefreshToken { ExpiresAt = expiresAt };
        token.ExpiresAt.Should().Be(expiresAt);
    }
}
