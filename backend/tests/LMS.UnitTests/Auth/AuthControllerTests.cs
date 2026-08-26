using FluentAssertions;
using LMS.Api.Controllers.V1;
using LMS.Api.Models.Requests;
using LMS.Api.Models.Responses;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LMS.UnitTests.Auth;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _authMock;
    private readonly AuthController _sut;
    private readonly DefaultHttpContext _httpContext;

    public AuthControllerTests()
    {
        _authMock = new Mock<IAuthService>();
        _sut = new AuthController(_authMock.Object);
        _httpContext = new DefaultHttpContext();
        _sut.ControllerContext = new ControllerContext { HttpContext = _httpContext };
    }

    private static AuthResult MakeAuthResult() =>
        new AuthResult
        {
            AccessToken = "test.access.token",
            RawRefreshToken = "raw-refresh-token",
            ExpiresInSeconds = 86400,
        };

    // UT-API-018
    [Fact]
    public async Task Login_ValidCredentials_Returns200WithApiResponseEnvelope()
    {
        _authMock
            .Setup(
                a =>
                    a.LoginAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(MakeAuthResult());

        var result = await _sut.Login(
            new LoginRequest { Email = "user@example.com", Password = "Password123!" }
        );

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        var envelope = ok.Value.Should().BeOfType<ApiResponse<TokenResponse>>().Subject;
        envelope.Data.AccessToken.Should().Be("test.access.token");
        envelope.Data.TokenType.Should().Be("Bearer");
        envelope.Data.ExpiresIn.Should().Be(86400);
    }

    // UT-API-019
    [Fact]
    public async Task Login_InvalidCredentials_Returns401ProblemDetails()
    {
        _authMock
            .Setup(
                a =>
                    a.LoginAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync((AuthResult?)null);

        var result = await _sut.Login(
            new LoginRequest { Email = "user@example.com", Password = "wrong" }
        );

        var unauth = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauth.StatusCode.Should().Be(401);
        unauth.Value.Should().BeOfType<ProblemDetails>();
    }

    // UT-API-020
    [Fact]
    public async Task Refresh_NoCookie_Returns401()
    {
        var result = await _sut.Refresh();
        var unauth = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauth.StatusCode.Should().Be(401);
    }

    // UT-API-021
    [Fact]
    public async Task Refresh_ValidCookie_Returns200WithNewTokens()
    {
        _httpContext.Request.Headers.Cookie = "lms_refresh_token=valid-token";
        _authMock
            .Setup(
                a =>
                    a.RefreshAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(MakeAuthResult());

        var result = await _sut.Refresh();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        var envelope = ok.Value.Should().BeOfType<ApiResponse<TokenResponse>>().Subject;
        envelope.Data.AccessToken.Should().Be("test.access.token");
    }

    // UT-API-022
    [Fact]
    public async Task Logout_Returns200WithDataTrue()
    {
        _authMock
            .Setup(a => a.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Logout();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        var envelope = ok.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        envelope.Data.Should().BeTrue();
    }
}
