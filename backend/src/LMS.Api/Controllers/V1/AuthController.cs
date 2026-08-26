using LMS.Api.Models.Requests;
using LMS.Api.Models.Responses;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "lms_refresh_token";
    private const int RefreshCookieMaxAgeSeconds = 7 * 24 * 60 * 60;

    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpGet("sso/login")]
    [AllowAnonymous]
    public IActionResult SsoLogin([FromQuery] string? state = null)
    {
        var url = _auth.GetSsoAuthorizationUrl(state);
        return Redirect(url);
    }

    [HttpGet("sso/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> SsoCallback(
        [FromQuery] string code,
        [FromQuery] string? state = null,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Missing code",
                    Detail = "The authorization code is required.",
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _auth.HandleSsoCallbackAsync(code, state, ip, ct);
        if (result is null)
        {
            return Unauthorized(
                new ProblemDetails
                {
                    Title = "SSO authentication failed",
                    Detail = "The SSO code was invalid, expired, or the user is not active.",
                    Status = StatusCodes.Status401Unauthorized,
                }
            );
        }

        SetRefreshTokenCookie(result.RawRefreshToken);
        return Ok(
            ApiResponse<TokenResponse>.Ok(
                new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    TokenType = "Bearer",
                    ExpiresIn = result.ExpiresInSeconds,
                }
            )
        );
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default
    )
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _auth.LoginAsync(request.Email, request.Password, ip, ct);
        if (result is null)
        {
            return Unauthorized(
                new ProblemDetails
                {
                    Title = "Authentication failed",
                    Detail = "The credentials are invalid or the account is locked.",
                    Status = StatusCodes.Status401Unauthorized,
                }
            );
        }

        SetRefreshTokenCookie(result.RawRefreshToken);
        return Ok(
            ApiResponse<TokenResponse>.Ok(
                new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    TokenType = "Bearer",
                    ExpiresIn = result.ExpiresInSeconds,
                }
            )
        );
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken ct = default)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var rawToken))
        {
            return Unauthorized(
                new ProblemDetails
                {
                    Title = "Missing refresh token",
                    Detail = "A valid refresh token cookie is required.",
                    Status = StatusCodes.Status401Unauthorized,
                }
            );
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _auth.RefreshAsync(rawToken, ip, ct);
        if (result is null)
        {
            Response.Cookies.Delete(RefreshTokenCookieName);
            return Unauthorized(
                new ProblemDetails
                {
                    Title = "Invalid refresh token",
                    Detail = "The refresh token is invalid, expired, or has been revoked.",
                    Status = StatusCodes.Status401Unauthorized,
                }
            );
        }

        SetRefreshTokenCookie(result.RawRefreshToken);
        return Ok(
            ApiResponse<TokenResponse>.Ok(
                new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    TokenType = "Bearer",
                    ExpiresIn = result.ExpiresInSeconds,
                }
            )
        );
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var rawToken))
        {
            await _auth.LogoutAsync(rawToken, ct);
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    private void SetRefreshTokenCookie(string rawToken)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            rawToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromSeconds(RefreshCookieMaxAgeSeconds),
                Path = "/api/v1/auth",
            }
        );
    }
}
