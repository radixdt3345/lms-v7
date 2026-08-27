using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LMS.Infrastructure.Common;
using LMS.Infrastructure.LeaveTypes;
using LMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.LeaveTypes;

/// <summary>
/// Integration tests for F-04 Leave Type and Policy Management.
/// Covers IT-025 through IT-030.
/// Each test seeds its own isolated users — no shared mutable state.
/// </summary>
[Collection("LeaveTypeIntegration")]
public sealed class LeaveTypeManagementTests : IAsyncLifetime
{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveTypeManagementTests(LmsWebApplicationFactory factory)
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

    private static string UniqueName(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";

    // ── IT-025: GET /api/v1/leave-types returns 5 seeded types ────────────────

    [Fact]
    public async Task IT025_GetLeaveTypes_ReturnsAtLeast5SeededTypes()
    {
        var empEmail = UniqueEmail("emp-025");
        await TestHelpers.SeedUserAsync(_factory, email: empEmail, role: "EMPLOYEE");
        var jwt = await LoginAsync(empEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.GetAsync("/api/v1/leave-types");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<List<LeaveTypeDto>>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Count.Should().BeGreaterThanOrEqualTo(5);

        var codes = body.Data.Select(t => t.Code).ToList();
        codes.Should().Contain("CL");
        codes.Should().Contain("SL");
        codes.Should().Contain("EL");
        codes.Should().Contain("CO");
        codes.Should().Contain("UL");
    }

    // ── IT-026: POST with HR_ADMIN JWT → 201 + LeaveTypeDto ──────────────────

    [Fact]
    public async Task IT026_CreateLeaveType_HrAdmin_Returns201WithDto()
    {
        var hrEmail = UniqueEmail("hr-026");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var name = UniqueName("Study Leave");
        var resp = await authed.PostAsJsonAsync("/api/v1/leave-types", new
        {
            name,
            code = $"SL{Guid.NewGuid():N}".Substring(0, 8).ToUpperInvariant(),
            description = "Leave for higher studies",
            annualDays = 5,
            requiresAttachment = true,
            requiresHrApproval = false,
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeDto>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Id.Should().NotBeEmpty();
        body.Data.Name.Should().Be(name);
        body.Data.IsActive.Should().BeTrue();
        body.Data.AnnualDays.Should().Be(5);
    }

    // ── IT-027: POST duplicate name → 409 DUPLICATE_LEAVE_TYPE ───────────────

    [Fact]
    public async Task IT027_CreateLeaveType_DuplicateName_Returns409()
    {
        var hrEmail = UniqueEmail("hr-027");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // First create
        var name = UniqueName("Dupe Leave");
        var code1 = $"D1{Guid.NewGuid():N}".Substring(0, 6).ToUpperInvariant();
        await authed.PostAsJsonAsync("/api/v1/leave-types", new
        {
            name, code = code1, annualDays = 0,
            requiresAttachment = false, requiresHrApproval = false,
        });

        // Duplicate name
        var code2 = $"D2{Guid.NewGuid():N}".Substring(0, 6).ToUpperInvariant();
        var dupeResp = await authed.PostAsJsonAsync("/api/v1/leave-types", new
        {
            name, code = code2, annualDays = 0,
            requiresAttachment = false, requiresHrApproval = false,
        });

        dupeResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await dupeResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("extensions").GetProperty("code").GetString()
            .Should().Be("DUPLICATE_LEAVE_TYPE");
    }

    // ── IT-028: POST with EMPLOYEE JWT → 403 ─────────────────────────────────

    [Fact]
    public async Task IT028_CreateLeaveType_EmployeeJwt_Returns403()
    {
        var empEmail = UniqueEmail("emp-028");
        await TestHelpers.SeedUserAsync(_factory, email: empEmail, role: "EMPLOYEE");
        var jwt = await LoginAsync(empEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PostAsJsonAsync("/api/v1/leave-types", new
        {
            name = "Forbidden Leave",
            code = "FB",
            annualDays = 0,
            requiresAttachment = false,
            requiresHrApproval = false,
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── IT-029: PUT /api/v1/leave-types/{id} with HR_ADMIN → 200 updated ─────

    [Fact]
    public async Task IT029_UpdateLeaveType_HrAdmin_Returns200Updated()
    {
        var hrEmail = UniqueEmail("hr-029");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create a leave type to update
        var createResp = await authed.PostAsJsonAsync("/api/v1/leave-types", new
        {
            name = UniqueName("Update Me"),
            code = $"UM{Guid.NewGuid():N}".Substring(0, 6).ToUpperInvariant(),
            annualDays = 3,
            requiresAttachment = false,
            requiresHrApproval = false,
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResp.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeDto>>())!.Data;

        // Update
        var updatedName = UniqueName("Updated Name");
        var updateResp = await authed.PutAsJsonAsync($"/api/v1/leave-types/{created.Id}", new
        {
            name = updatedName,
            annualDays = 7,
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await updateResp.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeDto>>();
        body!.Data.Name.Should().Be(updatedName);
        body.Data.AnnualDays.Should().Be(7);
    }

    // ── IT-030: DELETE /api/v1/leave-types/{id} with HR_ADMIN → 204 ──────────

    [Fact]
    public async Task IT030_DeactivateLeaveType_HrAdmin_Returns204()
    {
        var hrEmail = UniqueEmail("hr-030");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create then deactivate
        var createResp = await authed.PostAsJsonAsync("/api/v1/leave-types", new
        {
            name = UniqueName("Deactivate Me"),
            code = $"DM{Guid.NewGuid():N}".Substring(0, 6).ToUpperInvariant(),
            annualDays = 0,
            requiresAttachment = false,
            requiresHrApproval = false,
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResp.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeDto>>())!.Data;

        var deleteResp = await authed.DeleteAsync($"/api/v1/leave-types/{created.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify is_active = false
        var getResp = await authed.GetAsync($"/api/v1/leave-types/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await getResp.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeDto>>())!;
        body.Data.IsActive.Should().BeFalse();
    }
}

// ── Collection definition ──────────────────────────────────────────────────────

[CollectionDefinition("LeaveTypeIntegration")]
public sealed class LeaveTypeIntegrationCollection
    : ICollectionFixture<LmsWebApplicationFactory> { }
