using FluentAssertions;
using LMS.Infrastructure.Data.Entities;
using Xunit;

namespace LMS.UnitTests.Data.Entities;

/// <summary>
/// Unit tests for the User entity class.
/// Verifies default values, nullability, and navigation property initialisation
/// — these directly reflect the DB schema constraints (DEFAULT values, NULL vs NOT NULL).
/// </summary>
public sealed class UserEntityConfigurationTests
{
    [Fact]
    public void User_Id_IsGuid()
    {
        var user = new User();
        user.Id.Should().BeOfType<Guid>();
    }

    [Fact]
    public void User_Role_DefaultValue_IsEmployee()
    {
        // Matches DB column default and CHECK constraint allowed values
        var user = new User();
        user.Role.Should().Be("EMPLOYEE");
    }

    [Fact]
    public void User_Status_DefaultValue_IsActive()
    {
        // Matches DB column default and CHECK constraint allowed values
        var user = new User();
        user.Status.Should().Be("Active");
    }

    [Fact]
    public void User_FailedAttempts_DefaultValue_IsZero()
    {
        // Matches DB column DEFAULT 0; counter resets on successful login or admin unlock
        var user = new User();
        user.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public void User_PasswordHash_IsNullable_ForSsoOnlyAccounts()
    {
        // SSO-only accounts never set a password — PasswordHash must be nullable
        var user = new User { PasswordHash = null };
        user.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void User_DepartmentId_IsNullable()
    {
        // Super Admin users may not belong to a department
        var user = new User { DepartmentId = null };
        user.DepartmentId.Should().BeNull();
    }

    [Fact]
    public void User_LockedAt_IsNullable_WhenAccountIsNotLocked()
    {
        var user = new User();
        user.LockedAt.Should().BeNull();
    }

    [Fact]
    public void User_DeletedAt_IsNullable_SoftDeleteColumn()
    {
        // Non-null deleted_at marks the record as deactivated (GDPR soft-delete)
        var user = new User();
        user.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void User_RefreshTokens_InitialisedAsEmptyCollection()
    {
        // Navigation collection must never be null — guards against NullReferenceException
        var user = new User();
        user.RefreshTokens.Should().NotBeNull();
        user.RefreshTokens.Should().BeEmpty();
    }

    [Fact]
    public void User_Name_DefaultsToEmptyString()
    {
        var user = new User();
        user.Name.Should().Be(string.Empty);
    }

    [Fact]
    public void User_Email_DefaultsToEmptyString()
    {
        var user = new User();
        user.Email.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("EMPLOYEE")]
    [InlineData("MANAGER")]
    [InlineData("HR_ADMIN")]
    [InlineData("SUPER_ADMIN")]
    public void User_Role_AcceptsAllValidValues(string role)
    {
        // All values must match the DB CHECK constraint: ck_users_role
        var user = new User { Role = role };
        user.Role.Should().Be(role);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Locked")]
    public void User_Status_AcceptsAllValidValues(string status)
    {
        // All values must match the DB CHECK constraint: ck_users_status
        var user = new User { Status = status };
        user.Status.Should().Be(status);
    }
}
