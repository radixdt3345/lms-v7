using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.IntegrationTests.Infrastructure;

/// <summary>
/// Shared helpers for seeding data inside integration tests.
/// </summary>
public static class TestHelpers
{
    // ── Database seeding ─────────────────────────────────────────────────────

    public static async Task<User> SeedUserAsync(
        LmsWebApplicationFactory factory,
        string email = "user@example.com",
        string role = "EMPLOYEE",
        string status = "Active",
        string password = "Password1!",
        int failedAttempts = 0
    )
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Status = status,
            FailedAttempts = failedAttempts,
            LockedAt = status == "Locked" ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<User?> GetUserAsync(LmsWebApplicationFactory factory, Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        return await db.Users.FindAsync(userId);
    }

    public static async Task<int> GetRefreshTokenCountAsync(
        LmsWebApplicationFactory factory,
        Guid userId
    )
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        return await db.RefreshTokens.CountAsync(rt => rt.UserId == userId);
    }

    // ── Azure AD mock payload helpers ──────────────────────────────────────────

    /// <summary>
    /// Creates a base64url-encoded fake id_token payload with the given email.
    /// AuthService extracts the email claim from this without verifying the signature.
    /// </summary>
    public static string CreateFakeIdToken(string email)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(new { email });
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var b64 = Convert.ToBase64String(payloadBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        // id_token is header.payload.signature — only the payload is inspected by AuthService
        return $"eyJhbGciOiJSUzI1NiJ9.{b64}.fakesig";
    }

    public static string BuildAzureTokenResponse(string email) =>
        System.Text.Json.JsonSerializer.Serialize(
            new { id_token = CreateFakeIdToken(email), access_token = "az_access_test" }
        );
}
