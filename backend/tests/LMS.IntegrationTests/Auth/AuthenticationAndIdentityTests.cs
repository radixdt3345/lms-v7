using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LMS.Api.Models.Responses;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Common;
using LMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.Auth;

/// <summary>
/// Integration tests for F-01 Authentication &amp; Identity.
/// Covers IT-001 through IT-015 (matching AC-1 through AC-15 from the feature spec).
///
/// All tests share one PostgreSQL container via ICollectionFixture — the DB is not
/// reset between tests, so each test seeds its own uniquely-emailed users.
/// </summary>
[Collection("AuthIntegration")]
public sealed class AuthenticationAndIdentityTests : IAsyncLifetime
{
    private readonly LmsWebApplicationFactory _factory;

    // No-redirect client used for SSO login and general unauthenticated calls
    private readonly HttpClient _client;

    public AuthenticationAndIdentityTests(LmsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Log in and return the bearer JWT for an already-seeded user.</summary>
    private async Task<string> LoginAndGetJwtAsync(string email, string password)
    {
        var resp = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.OK, $"login failed for {email}");
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        return body!.Data.AccessToken;
    }

    /// <summary>Create an authenticated client with the given JWT pre-set.</summary>
    private HttpClient AuthedClient(string jwt)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return c;
    }

    // ── IT-001 / AC-1: SSO redirect ──────────────────────────────────────────

    /// <summary>
    /// IT-001: GET /api/v1/auth/sso/login returns HTTP 302 redirect to Azure AD authorize URL.
    /// </summary>
    [Fact]
    public async Task IT001_SsoLogin_Returns302RedirectToAzureAd()
    {
        var response = await _client.GetAsync("/api/v1/auth/sso/login");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response
            .Headers.Location!.ToString()
            .Should()
            .Contain("login.microsoftonline.com")
            .And.Contain("test-client-id");
    }

    // ── IT-002 / AC-5: Local login with valid credentials ────────────────────

    /// <summary>
    /// IT-002: POST /api/v1/auth/login with valid credentials returns HTTP 200
    /// with ApiResponse{ data: { accessToken, tokenType, expiresIn } }.
    /// </summary>
    [Fact]
    public async Task IT002_LocalLogin_ValidCredentials_Returns200WithTokens()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it002@example.com",
            password: "Secure1!",
            role: "EMPLOYEE"
        );

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "it002@example.com", password = "Secure1!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists — never .Items or .Result
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.TokenType.Should().Be("Bearer");
        body.Data.ExpiresIn.Should().BeGreaterThan(0);
    }

    // ── IT-003 / AC-6: Invalid password → 401 ───────────────────────────────

    /// <summary>
    /// IT-003: POST /api/v1/auth/login with wrong password returns HTTP 401.
    /// </summary>
    [Fact]
    public async Task IT003_LocalLogin_WrongPassword_Returns401()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it003@example.com",
            password: "Correct1!"
        );

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "it003@example.com", password = "WrongPass1!" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── IT-004 / AC-7: JWT payload claims ────────────────────────────────────

    /// <summary>
    /// IT-004: The access_token contains sub, role, and exp claims.
    /// </summary>
    [Fact]
    public async Task IT004_AccessToken_ContainsSubRoleExpClaims()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it004@example.com",
            role: "HR_ADMIN",
            password: "Claims1!"
        );

        var jwt = await LoginAndGetJwtAsync("it004@example.com", "Claims1!");

        var parts = jwt.Split('.');
        parts.Should().HaveCount(3, "JWT must have header.payload.signature");

        var padded = parts[1].Replace('-', '+').Replace('_', '/');
        while (padded.Length % 4 != 0) padded += "=";

        var payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        root.TryGetProperty("sub", out _).Should().BeTrue("JWT must have 'sub' claim");
        root.TryGetProperty("role", out var roleClaim).Should().BeTrue("JWT must have 'role' claim");
        root.TryGetProperty("exp", out var expClaim).Should().BeTrue("JWT must have 'exp' claim");

        roleClaim.GetString().Should().Be("HR_ADMIN");
        expClaim.GetInt64().Should().BeGreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    // ── IT-005 / AC-8: Logout returns 200 ────────────────────────────────────

    /// <summary>
    /// IT-005: POST /api/v1/auth/logout returns HTTP 200 with ApiResponse{ data: true }.
    /// </summary>
    [Fact]
    public async Task IT005_Logout_Returns200WithTrue()
    {
        await TestHelpers.SeedUserAsync(_factory, email: "it005@example.com", password: "Logout1!");
        await LoginAndGetJwtAsync("it005@example.com", "Logout1!");

        var logoutResp = await _client.PostAsync("/api/v1/auth/logout", null);
        logoutResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists
        var body = await logoutResp.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Data.Should().BeTrue();
    }

    // ── IT-006 / AC-11: 3 failed attempts lock the account ───────────────────

    /// <summary>
    /// IT-006: Three consecutive bad-password attempts lock the account;
    /// even the correct password returns 401 afterward.
    /// </summary>
    [Fact]
    public async Task IT006_ThreeFailedAttempts_LocksAccount_CorrectPasswordStillReturns401()
    {
        await TestHelpers.SeedUserAsync(_factory, email: "it006@example.com", password: "Lock1!");

        for (var i = 0; i < 3; i++)
        {
            var bad = await _client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new { email = "it006@example.com", password = "Wrong!" }
            );
            bad.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"attempt {i + 1}");
        }

        // Account is now locked — correct password must still return 401
        var lockedAttempt = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "it006@example.com", password = "Lock1!" }
        );
        lockedAttempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── IT-007 / AC-14: No JWT → 401 ─────────────────────────────────────────

    /// <summary>
    /// IT-007: GET /api/v1/accounts/locked without a bearer token returns HTTP 401.
    /// </summary>
    [Fact]
    public async Task IT007_ProtectedEndpoint_WithoutJwt_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/accounts/locked");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── IT-008 / AC-12: HR Admin lists locked accounts ───────────────────────

    /// <summary>
    /// IT-008: GET /api/v1/accounts/locked with an HR_ADMIN JWT returns HTTP 200
    /// with ApiResponse{ data: [...] }.
    /// </summary>
    [Fact]
    public async Task IT008_GetLockedAccounts_HrAdminToken_Returns200WithData()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it008admin@example.com",
            role: "HR_ADMIN",
            password: "Admin8!"
        );
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it008locked@example.com",
            role: "EMPLOYEE",
            status: "Locked",
            failedAttempts: 3
        );

        var jwt = await LoginAndGetJwtAsync("it008admin@example.com", "Admin8!");
        using var c = AuthedClient(jwt);

        var response = await c.GetAsync("/api/v1/accounts/locked");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists — never .Items or .Result
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<LockedUserResponse>>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Should().Contain(u => u.Email == "it008locked@example.com");
    }

    // ── IT-009 / AC-13: HR Admin unlocks an account ──────────────────────────

    /// <summary>
    /// IT-009: POST /api/v1/accounts/{id}/unlock sets Status=Active, FailedAttempts=0
    /// and returns HTTP 200 with ApiResponse{ data: true }.
    /// </summary>
    [Fact]
    public async Task IT009_UnlockAccount_HrAdminToken_SetsActiveAndReturns200()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it009admin@example.com",
            role: "HR_ADMIN",
            password: "Admin9!"
        );
        var lockedUser = await TestHelpers.SeedUserAsync(
            _factory,
            email: "it009locked@example.com",
            role: "EMPLOYEE",
            status: "Locked",
            failedAttempts: 3
        );

        var jwt = await LoginAndGetJwtAsync("it009admin@example.com", "Admin9!");
        using var c = AuthedClient(jwt);

        var response = await c.PostAsync($"/api/v1/accounts/{lockedUser.Id}/unlock", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        body.Should().NotBeNull();
        body!.Data.Should().BeTrue();

        // Verify the DB record is correctly updated
        var updated = await TestHelpers.GetUserAsync(_factory, lockedUser.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Active");
        updated.FailedAttempts.Should().Be(0);
        updated.LockedAt.Should().BeNull();
    }

    // ── IT-010 / AC-13: Unlock unknown user → 404 ────────────────────────────

    /// <summary>
    /// IT-010: POST /api/v1/accounts/{id}/unlock with an unknown GUID returns HTTP 404.
    /// </summary>
    [Fact]
    public async Task IT010_UnlockAccount_UnknownId_Returns404()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it010admin@example.com",
            role: "HR_ADMIN",
            password: "Admin10!"
        );

        var jwt = await LoginAndGetJwtAsync("it010admin@example.com", "Admin10!");
        using var c = AuthedClient(jwt);

        var response = await c.PostAsync($"/api/v1/accounts/{Guid.NewGuid()}/unlock", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── IT-011 / RBAC: EMPLOYEE cannot access HR Admin endpoints ─────────────

    /// <summary>
    /// IT-011: GET /api/v1/accounts/locked with an EMPLOYEE JWT returns HTTP 403.
    /// </summary>
    [Fact]
    public async Task IT011_GetLockedAccounts_EmployeeToken_Returns403()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it011emp@example.com",
            role: "EMPLOYEE",
            password: "Emp11!"
        );

        var jwt = await LoginAndGetJwtAsync("it011emp@example.com", "Emp11!");
        using var c = AuthedClient(jwt);

        var response = await c.GetAsync("/api/v1/accounts/locked");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── IT-012 / AC-2+AC-3: SSO callback returns tokens ─────────────────────

    /// <summary>
    /// IT-012: SSO callback with a valid code (mocked Azure AD) returns HTTP 200 with tokens.
    /// </summary>
    [Fact]
    public async Task IT012_SsoCallback_ValidCode_Returns200WithTokens()
    {
        var userEmail = "it012sso@example.com";
        await TestHelpers.SeedUserAsync(
            _factory,
            email: userEmail,
            role: "HR_ADMIN",
            status: "Active"
        );

        _factory.AzureAdHttpHandler.SetJsonResponse(
            TestHelpers.BuildAzureTokenResponse(userEmail)
        );

        using var ssoClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
        var response = await ssoClient.GetAsync("/api/v1/auth/sso/callback?code=test-auth-code");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.AccessToken.Should().NotBeNullOrWhiteSpace();

        _factory.AzureAdHttpHandler.Reset();
    }

    // ── IT-013 / AC-2: SSO callback with invalid code → 401 ─────────────────

    /// <summary>
    /// IT-013: SSO callback with an invalid/expired code (Azure AD returns error) → HTTP 401.
    /// </summary>
    [Fact]
    public async Task IT013_SsoCallback_InvalidCode_Returns401()
    {
        _factory.AzureAdHttpHandler.SetJsonResponse(
            "{\"error\":\"invalid_grant\",\"error_description\":\"AADSTS70011\"}",
            HttpStatusCode.BadRequest
        );

        using var ssoClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
        var response = await ssoClient.GetAsync("/api/v1/auth/sso/callback?code=expired-code");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        _factory.AzureAdHttpHandler.Reset();
    }

    // ── IT-014 / AC-15: Refresh token rotation ───────────────────────────────

    /// <summary>
    /// IT-014: POST /api/v1/auth/refresh with a valid refresh cookie returns new tokens.
    /// </summary>
    [Fact]
    public async Task IT014_RefreshToken_ValidCookie_ReturnsNewTokenPair()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it014@example.com",
            password: "Refresh14!"
        );

        using var cookieClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            }
        );

        var loginResp = await cookieClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "it014@example.com", password = "Refresh14!" }
        );
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResp = await cookieClient.PostAsync("/api/v1/auth/refresh", null);
        refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists
        var body = await refreshResp.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    // ── IT-015 / AC-15: Revoked refresh token → 401 ──────────────────────────

    /// <summary>
    /// IT-015: POST /api/v1/auth/refresh after logout returns HTTP 401.
    /// </summary>
    [Fact]
    public async Task IT015_RefreshToken_AfterLogout_Returns401()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "it015@example.com",
            password: "Revoke15!"
        );

        using var cookieClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            }
        );

        await cookieClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "it015@example.com", password = "Revoke15!" }
        );

        await cookieClient.PostAsync("/api/v1/auth/logout", null);

        var refreshResp = await cookieClient.PostAsync("/api/v1/auth/refresh", null);
        refreshResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

// ── xUnit collection fixture ──────────────────────────────────────────────────

[CollectionDefinition("AuthIntegration")]
public sealed class AuthIntegrationCollection : ICollectionFixture<LmsWebApplicationFactory> { }

// ── Local response DTO for deserialization ─────────────────────────────────────
internal sealed class LockedUserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
    public DateTime? LockedAt { get; set; }
}
