using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LMS.Api.Models.Responses;
using LMS.Infrastructure.Common;
using LMS.Infrastructure.Employees;
using LMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LMS.IntegrationTests.Employees;

/// <summary>
/// Integration tests for F-02 Employee Management.
/// Covers IT-016 through IT-024 (AC-16 through AC-24).
/// Each test seeds its own isolated users — no shared mutable state.
/// </summary>
[Collection("EmployeeIntegration")]
public sealed class EmployeeManagementTests : IAsyncLifetime
{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeeManagementTests(LmsWebApplicationFactory factory)
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

    // ── IT-016 / AC-16: Create employee (HR_ADMIN) → 201 ─────────────────────

    [Fact]
    public async Task IT016_CreateEmployee_HrAdmin_Returns201WithDto()
    {
        var hrEmail = UniqueEmail("hr-016");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var newEmpEmail = UniqueEmail("emp-016");
        var resp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Alice Test",
            email = newEmpEmail,
            phone = "+1-555-0101",
            jobTitle = "Engineer",
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Id.Should().NotBeEmpty();
        body.Data.Email.Should().Be(newEmpEmail);
        body.Data.Status.Should().Be("Active");
    }

    // ── IT-017 / AC-17: Create without reportingManagerId → null manager ──────

    [Fact]
    public async Task IT017_CreateEmployee_NoManager_Returns201WithNullManagerId()
    {
        var hrEmail = UniqueEmail("hr-017");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Bob NoManager",
            email = UniqueEmail("emp-017"),
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        body.Should().NotBeNull();
        body!.Data.ReportingManagerId.Should().BeNull();
    }

    // ── IT-018 / AC-18: Manager cannot create employees → 403 ────────────────

    [Fact]
    public async Task IT018_CreateEmployee_ManagerJwt_Returns403()
    {
        var mgrEmail = UniqueEmail("mgr-018");
        await TestHelpers.SeedUserAsync(_factory, email: mgrEmail, role: "MANAGER");
        var jwt = await LoginAsync(mgrEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Forbidden Emp",
            email = UniqueEmail("emp-018"),
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── IT-019 / AC-19: Soft-delete — status=Inactive, still retrievable ──────

    [Fact]
    public async Task IT019_DeactivateEmployee_Returns204_StillRetrievableAsInactive()
    {
        var hrEmail = UniqueEmail("hr-019");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        var createResp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Deactivate Me",
            email = UniqueEmail("emp-019"),
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;

        var deleteResp = await authed.DeleteAsync($"/api/v1/employees/{created.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await authed.GetAsync($"/api/v1/employees/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        body!.Data.Status.Should().Be("Inactive");
    }

    // ── IT-020 / AC-20: Auto-promote EMPLOYEE → MANAGER ──────────────────────

    [Fact]
    public async Task IT020_CreateWithReportingManager_AutoPromotesManagerRole()
    {
        var hrEmail = UniqueEmail("hr-020");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create future manager as EMPLOYEE via API
        var futMgrEmail = UniqueEmail("fut-mgr-020");
        var mgrCreateResp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Future Manager",
            email = futMgrEmail,
        });
        mgrCreateResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var futMgr = (await mgrCreateResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;
        futMgr.Role.Should().Be("EMPLOYEE");

        // Create direct report → triggers auto-promote
        var drResp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Direct Report",
            email = UniqueEmail("dr-020"),
            reportingManagerId = futMgr.Id,
        });
        drResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify auto-promoted to MANAGER
        var getMgrResp = await authed.GetAsync($"/api/v1/employees/{futMgr.Id}");
        getMgrResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedMgr = (await getMgrResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;
        updatedMgr.Role.Should().Be("MANAGER");
    }

    // ── IT-021 / AC-21: Auto-demote MANAGER → EMPLOYEE when last report removed ─

    [Fact]
    public async Task IT021_RemoveLastDirectReport_AutoDemotesManagerToEmployee()
    {
        var hrEmail = UniqueEmail("hr-021");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create manager
        var managerResp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "To Be Demoted",
            email = UniqueEmail("mgr-021"),
        });
        var manager = (await managerResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;

        // Assign direct report (promotes manager)
        var reportResp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Direct Report 021",
            email = UniqueEmail("dr-021"),
            reportingManagerId = manager.Id,
        });
        var report = (await reportResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;

        // Verify promoted
        var afterPromote = (await (await authed.GetAsync($"/api/v1/employees/{manager.Id}"))
            .Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;
        afterPromote.Role.Should().Be("MANAGER");

        // Clear reporting manager from the direct report → triggers auto-demote
        var updateResp = await authed.PutAsJsonAsync($"/api/v1/employees/{report.Id}", new
        {
            clearReportingManager = true,
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify demoted
        var afterDemote = (await (await authed.GetAsync($"/api/v1/employees/{manager.Id}"))
            .Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;
        afterDemote.Role.Should().Be("EMPLOYEE");
    }

    // ── IT-022 / AC-22: Manual demotion with active reports → 409 ─────────────

    [Fact]
    public async Task IT022_ManuallyDemoteManagerWithActiveReports_Returns409()
    {
        var hrEmail = UniqueEmail("hr-022");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var jwt = await LoginAsync(hrEmail);
        using var authed = AuthedClient(jwt);

        // Create manager
        var managerResp = await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Active Manager",
            email = UniqueEmail("mgr-022"),
        });
        var manager = (await managerResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;

        // Assign direct report → promotes manager
        await authed.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Active Report",
            email = UniqueEmail("dr-022"),
            reportingManagerId = manager.Id,
        });

        // Attempt manual demotion → 409
        var demoteResp = await authed.PutAsJsonAsync($"/api/v1/employees/{manager.Id}", new
        {
            role = "EMPLOYEE",
        });

        demoteResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await demoteResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("extensions").GetProperty("code").GetString()
            .Should().Be("MANAGER_HAS_ACTIVE_REPORTS");
    }

    // ── IT-023 / AC-23: Self-edit name + phone → 200 ─────────────────────────

    [Fact]
    public async Task IT023_SelfEdit_NameAndPhone_Returns200WithUpdatedValues()
    {
        var empEmail = UniqueEmail("emp-023");
        await TestHelpers.SeedUserAsync(_factory, email: empEmail, role: "EMPLOYEE");
        var jwt = await LoginAsync(empEmail);
        using var authed = AuthedClient(jwt);

        var resp = await authed.PutAsJsonAsync("/api/v1/employees/me", new
        {
            name = "Updated Name",
            phone = "+1-999-0023",
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
        body.Should().NotBeNull();
        body!.Data.Name.Should().Be("Updated Name");
        body.Data.Phone.Should().Be("+1-999-0023");
        body.Data.Email.Should().Be(empEmail);
        body.Data.Role.Should().Be("EMPLOYEE");
    }

    // ── IT-024 / AC-24: Get team (Manager with direct reports) → 200 ──────────

    [Fact]
    public async Task IT024_GetTeam_ManagerWithDirectReports_Returns200NonEmptyList()
    {
        var hrEmail = UniqueEmail("hr-024");
        await TestHelpers.SeedUserAsync(_factory, email: hrEmail, role: "HR_ADMIN");
        var hrJwt = await LoginAsync(hrEmail);
        using var hrClient = AuthedClient(hrJwt);

        // Create future manager via API
        var mgrEmail = UniqueEmail("mgr-024");
        var mgrCreateResp = await hrClient.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Future Manager 024",
            email = mgrEmail,
        });
        mgrCreateResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var mgrRecord = (await mgrCreateResp.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>())!.Data;

        // Seed the manager user's password so they can log in
        // (API-created employees need a password via TestHelpers to allow login)
        await TestHelpers.SetUserPasswordAsync(_factory, mgrRecord.Id, "Password1!");

        // Create direct report (promotes manager)
        await hrClient.PostAsJsonAsync("/api/v1/employees", new
        {
            name = "Team Member 024",
            email = UniqueEmail("tm-024"),
            reportingManagerId = mgrRecord.Id,
        });

        // Login as the now-promoted manager
        var mgrJwt = await LoginAsync(mgrEmail);
        using var mgrClient = AuthedClient(mgrJwt);

        var teamResp = await mgrClient.GetAsync("/api/v1/employees/team");
        teamResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await teamResp.Content.ReadFromJsonAsync<ApiResponse<List<EmployeeDto>>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeEmpty();
    }
}

// ── Collection definition ─────────────────────────────────────────────────────

[CollectionDefinition("EmployeeIntegration")]
public sealed class EmployeeIntegrationCollection
    : ICollectionFixture<LmsWebApplicationFactory> { }
