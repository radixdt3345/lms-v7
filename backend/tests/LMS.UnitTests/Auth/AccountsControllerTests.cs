using FluentAssertions;
using LMS.Api.Controllers.V1;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.UnitTests.Auth;

public sealed class AccountsControllerTests
{
    private readonly Mock<IAccountService> _accountsMock;
    private readonly AccountsController _sut;

    public AccountsControllerTests()
    {
        _accountsMock = new Mock<IAccountService>();
        _sut = new AccountsController(_accountsMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    // UT-API-023
    [Fact]
    public async Task GetLockedAccounts_Returns200WithApiResponseEnvelope()
    {
        var locked = new List<LockedUserDto>
        {
            new LockedUserDto
            {
                Id = Guid.NewGuid(),
                Name = "Locked User",
                Email = "locked@example.com",
                Role = "EMPLOYEE",
                FailedAttempts = 3,
                LockedAt = DateTime.UtcNow.AddHours(-1),
            },
        };
        _accountsMock
            .Setup(a => a.GetLockedUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locked);

        var result = await _sut.GetLockedAccounts();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        var envelope = ok.Value
            .Should()
            .BeOfType<ApiResponse<IReadOnlyList<LockedUserDto>>>()
            .Subject;
        envelope.Data.Should().HaveCount(1);
        envelope.Data[0].Email.Should().Be("locked@example.com");
    }

    // UT-API-024
    [Fact]
    public async Task GetLockedAccounts_NoLockedUsers_ReturnsEmptyData()
    {
        _accountsMock
            .Setup(a => a.GetLockedUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LockedUserDto>());

        var result = await _sut.GetLockedAccounts();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = ok.Value
            .Should()
            .BeOfType<ApiResponse<IReadOnlyList<LockedUserDto>>>()
            .Subject;
        envelope.Data.Should().BeEmpty();
    }

    // UT-API-025
    [Fact]
    public async Task UnlockAccount_LockedUser_Returns200WithDataTrue()
    {
        var userId = Guid.NewGuid();
        _accountsMock
            .Setup(a => a.UnlockUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UnlockAccount(userId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        var envelope = ok.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        envelope.Data.Should().BeTrue();
    }

    // UT-API-026
    [Fact]
    public async Task UnlockAccount_NonExistentUser_Returns404ProblemDetails()
    {
        var userId = Guid.NewGuid();
        _accountsMock
            .Setup(a => a.UnlockUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UnlockAccount(userId);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(404);
        notFound.Value.Should().BeOfType<ProblemDetails>();
    }
}
