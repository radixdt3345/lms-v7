using FluentAssertions;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LMS.UnitTests.Auth;

public sealed class AuthServiceTests : IDisposable
{
    private readonly LmsDbContext _db;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<LmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LmsDbContext(options);

        _jwtMock = new Mock<IJwtService>();
        _jwtMock
            .Setup(
                j =>
                    j.GenerateAccessTokenAsync(
                        It.IsAny<User>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync("mocked.access.token");
        _jwtMock.Setup(j => j.GenerateRawRefreshToken()).Returns("mocked-raw-refresh-token");

        var configValues = new Dictionary<string, string?>
        {
            ["AzureAd__TenantId"] = "test-tenant",
            ["AzureAd__ClientId"] = "test-client-id",
            ["AzureAd__ClientSecret"] = "test-secret",
            ["AzureAd__RedirectUri"] = "https://localhost/api/v1/auth/sso/callback",
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        _sut = new AuthService(_db, _jwtMock.Object, config, new HttpClient());
    }

    public void Dispose() => _db.Dispose();

    private async Task<User> SeedActiveUserAsync(
        string email = "user@example.com",
        string status = "Active",
        string? passwordHash = null
    )
    {
        var hash = passwordHash ?? BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = email,
            PasswordHash = hash,
            Role = "EMPLOYEE",
            Status = status,
            FailedAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    // UT-API-006
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResult()
    {
        await SeedActiveUserAsync();
        var result = await _sut.LoginAsync("user@example.com", "Password123!", "127.0.0.1");

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("mocked.access.token");
        result.RawRefreshToken.Should().Be("mocked-raw-refresh-token");
    }

    // UT-API-007
    [Fact]
    public async Task LoginAsync_WrongPassword_IncrementsFailedAttempts()
    {
        var user = await SeedActiveUserAsync();
        var result = await _sut.LoginAsync("user@example.com", "WrongPassword!", "127.0.0.1");

        result.Should().BeNull();
        var updated = await _db.Users.FindAsync(user.Id);
        updated!.FailedAttempts.Should().Be(1);
    }

    // UT-API-008
    [Fact]
    public async Task LoginAsync_ThreeConsecutiveFailures_LocksAccount()
    {
        var user = await SeedActiveUserAsync();
        var ip = "127.0.0.1";

        await _sut.LoginAsync(user.Email, "bad", ip);
        await _sut.LoginAsync(user.Email, "bad", ip);
        await _sut.LoginAsync(user.Email, "bad", ip);

        var updated = await _db.Users.FindAsync(user.Id);
        updated!.Status.Should().Be("Locked");
        updated.LockedAt.Should().NotBeNull();
        updated.FailedAttempts.Should().Be(3);
    }

    // UT-API-009
    [Fact]
    public async Task LoginAsync_LockedAccount_ReturnsNull()
    {
        var user = await SeedActiveUserAsync(status: "Locked");
        var result = await _sut.LoginAsync(user.Email, "Password123!", "127.0.0.1");
        result.Should().BeNull();
    }

    // UT-API-010
    [Fact]
    public async Task LoginAsync_SuccessAfterFailures_ResetsFailedAttempts()
    {
        var user = await SeedActiveUserAsync();
        user.FailedAttempts = 2;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var result = await _sut.LoginAsync(user.Email, "Password123!", "127.0.0.1");

        result.Should().NotBeNull();
        var updated = await _db.Users.FindAsync(user.Id);
        updated!.FailedAttempts.Should().Be(0);
    }

    // UT-API-011
    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokensAndRevokesOld()
    {
        var user = await SeedActiveUserAsync();
        var rawToken = "valid-raw-token";
        var tokenHash = JwtService.HashRefreshToken(rawToken);

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.RefreshAsync(rawToken, "127.0.0.1");

        result.Should().NotBeNull();
        var old = await _db.RefreshTokens.FirstAsync(rt => rt.TokenHash == tokenHash);
        old.RevokedAt.Should().NotBeNull();
    }

    // UT-API-012
    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsNull()
    {
        var user = await SeedActiveUserAsync();
        var rawToken = "expired-token";
        var tokenHash = JwtService.HashRefreshToken(rawToken);

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.RefreshAsync(rawToken, "127.0.0.1");
        result.Should().BeNull();
    }

    // UT-API-013
    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        var user = await SeedActiveUserAsync();
        var rawToken = "logout-token";
        var tokenHash = JwtService.HashRefreshToken(rawToken);

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        await _db.SaveChangesAsync();

        await _sut.LogoutAsync(rawToken);

        var stored = await _db.RefreshTokens.FirstAsync(rt => rt.TokenHash == tokenHash);
        stored.RevokedAt.Should().NotBeNull();
    }
}
