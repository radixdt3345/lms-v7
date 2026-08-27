using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LMS.Api.Models.Responses;
using LMS.Infrastructure.AuditLogs;
using LMS.Infrastructure.Common;
using LMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.AuditLogs;

/// <summary>
/// Integration tests for F-13 Audit Trail.
/// Covers IT-064 through IT-067.
/// Each test seeds its own isolated users — no shared mutable state.
/// </summary>
[Collection("AuditLogIntegration")]
public sealed class AuditTrailTests : IAsyncLifetime
{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuditTrailTests(LmsWebApplicationFactory factory)
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

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<string> LoginAsync(string email, string password = "Password1!")
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        resp.StatusCode.Should().Be(HttpStatusCode.OK, $"login failed for {email}");
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        return body!.Data.AccessToken;
    }

    private HttpClient AuthedClient(string jwt)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return c;
    }

    private static string UniqueEmail(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}@example.com";

    // ── IT-064: GET /api/v1/audit-log with HR_ADMIN → 200 ────────────────────

    /// <summary>
    /// IT-064: HR_ADMIN can call GET /api/v1/audit-log and receives a valid
    /// paged result structure even when the log is empty.
    /// </summary>
    [Fact]
    public async Task IT064_GetAuditLog_HrAdmin_Returns200WithPagedStructure()
    {
        var hrEmail = UniqueEmail("hr-064");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.GetAsync("/api/v1/audit-log");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AuditLogDto>>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Items.Should().NotBeNull();
        body.Data.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        body.Data.Page.Should().BeGreaterThan(0);
        body.Data.PageSize.Should().BeGreaterThan(0);
    }

    // ── IT-065: PUT /api/v1/audit-log → 405 (AC-65) ──────────────────────────

    /// <summary>
    /// IT-065: PUT /api/v1/audit-log is rejected with 405 Method Not Allowed
    /// (audit log is append-only per AC-65).
    /// </summary>
    [Fact]
    public async Task IT065_PutAuditLog_Returns405MethodNotAllowed()
    {
        var hrEmail = UniqueEmail("hr-065");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PutAsJsonAsync("/api/v1/audit-log", new { });

        resp.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    // ── IT-066: DELETE /api/v1/audit-log → 405 ────────────────────────────────

    /// <summary>
    /// IT-066: DELETE /api/v1/audit-log is rejected with 405 Method Not Allowed
    /// (audit log is immutable).
    /// </summary>
    [Fact]
    public async Task IT066_DeleteAuditLog_Returns405MethodNotAllowed()
    {
        var hrEmail = UniqueEmail("hr-066");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.DeleteAsync("/api/v1/audit-log");

        resp.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    // ── IT-067: EMPLOYEE JWT → 403 ────────────────────────────────────────────

    /// <summary>
    /// IT-067: An EMPLOYEE-role JWT cannot access GET /api/v1/audit-log;
    /// the response must be 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task IT067_GetAuditLog_EmployeeRole_Returns403()
    {
        var empEmail = UniqueEmail("emp-067");
        await TestHelpers.SeedUserAsync(_factory, email: empEmail, role: "EMPLOYEE");
        var jwt = await LoginAsync(empEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.GetAsync("/api/v1/audit-log");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

// ── Collection definition ──────────────────────────────────────────────────────
[CollectionDefinition("AuditLogIntegration")]
public sealed class AuditLogIntegrationCollection
    : ICollectionFixture<LmsWebApplicationFactory> { }
