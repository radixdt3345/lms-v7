using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LMS.Api.Models.Responses;
using LMS.Infrastructure.Common;
using LMS.Infrastructure.PublicHolidays;
using LMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.PublicHolidays;

/// <summary>
/// Integration tests for F-10 Public Holiday Management.
/// Covers IT-055 through IT-060.
/// Each test seeds its own isolated users — no shared mutable state.
/// </summary>
[Collection("PublicHolidayIntegration")]
public sealed class PublicHolidayTests : IAsyncLifetime
{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicHolidayTests(LmsWebApplicationFactory factory)
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

    private async Task<string> LoginAsync(string email, string password = "Password1!")
    {
        var resp = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password }
        );
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

    // ── IT-055: GET /api/v1/holidays?year=2026 (no auth) → 200 ───────────────

    [Fact]
    public async Task IT055_ListHolidays_NoAuth_Returns200WithList()
    {
        var resp = await _client.GetAsync("/api/v1/holidays?year=2026");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<List<PublicHolidayDto>>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
    }

    // ── IT-056: POST /api/v1/holidays with HR_ADMIN → 201 ────────────────────

    [Fact]
    public async Task IT056_CreateHoliday_HrAdmin_Returns201WithDto()
    {
        var hrEmail = UniqueEmail("hr-056");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PostAsJsonAsync("/api/v1/holidays", new
        {
            date = "2026-12-25",
            name = "Christmas Day IT056"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<PublicHolidayDto>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Id.Should().NotBeEmpty();
        body.Data.Name.Should().Be("Christmas Day IT056");
        body.Data.Year.Should().Be(2026);
    }

    // ── IT-057: POST /api/v1/holidays without auth → 401 ─────────────────────

    [Fact]
    public async Task IT057_CreateHoliday_NoAuth_Returns401()
    {
        var resp = await _client.PostAsJsonAsync("/api/v1/holidays", new
        {
            date = "2026-11-11",
            name = "Unauthorized Holiday"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── IT-058: PUT /api/v1/holidays/{id} with HR_ADMIN → 200 ────────────────

    [Fact]
    public async Task IT058_UpdateHoliday_HrAdmin_Returns200WithUpdatedName()
    {
        var hrEmail = UniqueEmail("hr-058");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create first
        var createResp = await authed.PostAsJsonAsync("/api/v1/holidays", new
        {
            date = "2026-01-01",
            name = "New Year IT058"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResp.Content.ReadFromJsonAsync<ApiResponse<PublicHolidayDto>>())!.Data;

        // Update
        var updateResp = await authed.PutAsJsonAsync($"/api/v1/holidays/{created.Id}", new
        {
            name = "New Year Updated IT058"
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await updateResp.Content.ReadFromJsonAsync<ApiResponse<PublicHolidayDto>>();
        body.Should().NotBeNull();
        body!.Data.Name.Should().Be("New Year Updated IT058");
    }

    // ── IT-059: DELETE /api/v1/holidays/{id} with HR_ADMIN → 204 ─────────────

    [Fact]
    public async Task IT059_DeleteHoliday_HrAdmin_Returns204()
    {
        var hrEmail = UniqueEmail("hr-059");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create first
        var createResp = await authed.PostAsJsonAsync("/api/v1/holidays", new
        {
            date = "2026-06-15",
            name = "Delete Me IT059"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResp.Content.ReadFromJsonAsync<ApiResponse<PublicHolidayDto>>())!.Data;

        // Delete
        var deleteResp = await authed.DeleteAsync($"/api/v1/holidays/{created.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── IT-060: POST bulk-import with HR_ADMIN, confirm=false → 200 preview ──

    [Fact]
    public async Task IT060_BulkImport_HrAdmin_ConfirmFalse_Returns200Preview()
    {
        var hrEmail = UniqueEmail("hr-060");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PostAsJsonAsync("/api/v1/holidays/bulk-import", new
        {
            year = 2026,
            holidays = new[]
            {
                new { date = "2026-03-17", name = "St. Patrick's Day IT060" },
                new { date = "2026-07-04", name = "Independence Day IT060" }
            },
            confirm = false
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<BulkImportPreview>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.ToCreate.Should().NotBeEmpty();
        body.Data.ToSkip.Should().BeEmpty();
        body.Data.Total.Should().Be(2);
    }
}

// ── Collection definition ─────────────────────────────────────────────────────

[CollectionDefinition("PublicHolidayIntegration")]
public sealed class PublicHolidayIntegrationCollection
    : ICollectionFixture<LmsWebApplicationFactory> { }
