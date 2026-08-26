using FluentAssertions;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LMS.UnitTests.Auth;

public sealed class AccountServiceTests : IDisposable
{
    private readonly LmsDbContext _db;
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<LmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LmsDbContext(options);
        _sut = new AccountService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<User> SeedUserAsync(
        string status = "Active",
        int failedAttempts = 0,
        DateTime? lockedAt = null
    )
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = $"test{Guid.NewGuid()}@example.com",
            Role = "EMPLOYEE",
            Status = status,
            FailedAttempts = failedAttempts,
            LockedAt = lockedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    // UT-API-014
    [Fact]
    public async Task GetLockedUsersAsync_ReturnsOnlyLockedUsers()
    {
        await SeedUserAsync(status: "Locked", failedAttempts: 3, lockedAt: DateTime.UtcNow);
        await SeedUserAsync(status: "Active");
        await SeedUserAsync(status: "Inactive");

        var result = await _sut.GetLockedUsersAsync();

        result.Should().HaveCount(1);
        result[0].FailedAttempts.Should().Be(3);
        result[0].LockedAt.Should().NotBeNull();
    }

    // UT-API-015
    [Fact]
    public async Task GetLockedUsersAsync_ExcludesDeletedUsers()
    {
        _db.Users.Add(
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Deleted Locked",
                Email = "deleted@example.com",
                Role = "EMPLOYEE",
                Status = "Locked",
                FailedAttempts = 3,
                LockedAt = DateTime.UtcNow,
                DeletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.GetLockedUsersAsync();
        result.Should().BeEmpty();
    }

    // UT-API-016
    [Fact]
    public async Task UnlockUserAsync_LockedUser_UnlocksAndResetState()
    {
        var user = await SeedUserAsync(
            status: "Locked",
            failedAttempts: 3,
            lockedAt: DateTime.UtcNow
        );

        var result = await _sut.UnlockUserAsync(user.Id);

        result.Should().BeTrue();
        var updated = await _db.Users.FindAsync(user.Id);
        updated!.Status.Should().Be("Active");
        updated.FailedAttempts.Should().Be(0);
        updated.LockedAt.Should().BeNull();
    }

    // UT-API-017
    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    public async Task UnlockUserAsync_NonLockedUser_ReturnsFalse(string status)
    {
        var user = await SeedUserAsync(status: status);
        var result = await _sut.UnlockUserAsync(user.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnlockUserAsync_NonExistentUser_ReturnsFalse()
    {
        var result = await _sut.UnlockUserAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }
}
