using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Common;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using LMS.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LMS.IntegrationTests.Departments;

/// <summary>
/// Integration tests for F-03 Department Management.
/// Covers AC-25 (RBAC), AC-26 (duplicate name), and AC-27 (audit trail).
///
/// IT-D-001: POST /api/v1/departments (EMPLOYEE JWT) → HTTP 403
/// IT-D-002: POST with duplicate dept name (same case) → HTTP 409 DUPLICATE_DEPARTMENT_NAME
/// IT-D-003: POST with duplicate dept name (case-insensitive) → HTTP 409
/// IT-D-004: Create dept → AuditLog entry (EntityType=Department, Action=CREATE)
/// IT-D-005: Update dept → AuditLog entry (Action=UPDATE)
/// IT-D-006: Deactivate dept → AuditLog entry (Action=DEACTIVATE) + soft-delete verified
/// IT-D-007: GET /api/v1/departments → HTTP 200 with ApiResponse{ data: [...] }
/// IT-D-008: POST creates dept → HTTP 201 with ApiResponse{ data: { id, name, code, ... } }
///
/// All tests seed their own isolated data via unique suffixes — no shared mutable state.
/// </summary>
[Collection("DepartmentIntegration")]
public sealed class DepartmentManagementTests : IAsyncLifetime
{
    private readonly LmsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DepartmentManagementTests(LmsWebApplicationFactory factory)
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

    /// <summary>Create an authenticated HttpClient with the given JWT pre-set.</summary>
    private HttpClient AuthedClient(string jwt)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return c;
    }

    /// <summary>Seed an HR_ADMIN user and return its JWT.</summary>
    private async Task<string> SeedAdminAndGetJwtAsync(string tag)
    {
        var email = $"admin-dept-{tag}@example.com";
        await TestHelpers.SeedUserAsync(
            _factory,
            email: email,
            role: "HR_ADMIN",
            password: "Admin1!"
        );
        return await LoginAndGetJwtAsync(email, "Admin1!");
    }

    /// <summary>Directly seed a Department entity in the DB (bypasses HTTP layer).</summary>
    private async Task<Department> SeedDepartmentAsync(string name, string code)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        var dept = new Department
        {
            Name = name,
            Code = code.ToUpperInvariant(),
            OverlapLimit = 2,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Departments.Add(dept);
        await db.SaveChangesAsync();
        return dept;
    }

    /// <summary>Query AuditLog rows for a given department entity ID.</summary>
    private async Task<List<AuditLog>> GetAuditLogsForEntityAsync(string entityId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        return await db.AuditLogs
            .Where(a => a.EntityId == entityId && a.EntityType == "Department")
            .ToListAsync();
    }

    // ── IT-D-001 / AC-25: EMPLOYEE JWT → 403 ──────────────────────────────────

    /// <summary>
    /// IT-D-001: POST /api/v1/departments with an EMPLOYEE JWT returns HTTP 403.
    /// Only HR_ADMIN and SUPER_ADMIN may create departments.
    /// </summary>
    [Fact]
    public async Task ITD001_CreateDepartment_EmployeeJwt_Returns403()
    {
        await TestHelpers.SeedUserAsync(
            _factory,
            email: "itd001emp@example.com",
            role: "EMPLOYEE",
            password: "Emp1!"
        );
        var jwt = await LoginAndGetJwtAsync("itd001emp@example.com", "Emp1!");
        using var c = AuthedClient(jwt);

        var response = await c.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "ITD001 Department",
            code = "ITD001",
            overlapLimit = 2,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── IT-D-002 / AC-26: Duplicate name (same case) → 409 ────────────────────

    /// <summary>
    /// IT-D-002: POST with a department name that already exists returns HTTP 409.
    /// </summary>
    [Fact]
    public async Task ITD002_CreateDepartment_DuplicateName_Returns409()
    {
        var jwt = await SeedAdminAndGetJwtAsync("002");
        await SeedDepartmentAsync("Engineering ITD002", "ENGITD002");

        using var c = AuthedClient(jwt);
        var response = await c.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Engineering ITD002",
            code = "ENGITD002X",
            overlapLimit = 2,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── IT-D-003 / AC-26: Duplicate name (case-insensitive) → 409 ─────────────

    /// <summary>
    /// IT-D-003: POST with a dept name that matches an existing name case-insensitively
    /// returns HTTP 409 (ILike match in DepartmentService).
    /// </summary>
    [Fact]
    public async Task ITD003_CreateDepartment_DuplicateNameCaseInsensitive_Returns409()
    {
        var jwt = await SeedAdminAndGetJwtAsync("003");
        await SeedDepartmentAsync("Marketing ITD003", "MKTITD003");

        using var c = AuthedClient(jwt);
        // "MARKETING ITD003" (all-caps) must collide via ILike
        var response = await c.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "MARKETING ITD003",
            code = "MKTITD003X",
            overlapLimit = 2,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ── IT-D-004 / AC-27: Create → AuditLog entry ─────────────────────────────

    /// <summary>
    /// IT-D-004: POST /api/v1/departments produces an AuditLog row with
    /// EntityType="Department", Action="CREATE", EntityId matching the new dept's Id.
    /// </summary>
    [Fact]
    public async Task ITD004_CreateDepartment_WritesAuditLogCreateEntry()
    {
        var jwt = await SeedAdminAndGetJwtAsync("004");
        using var c = AuthedClient(jwt);

        var response = await c.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "HR Department ITD004",
            code = "HRITD004",
            overlapLimit = 3,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // ⚠️ Assert ApiResponse<T>.Data exists — never .Items or .Result
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeptResponse>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Id.Should().NotBeEmpty();
        body.Data.Name.Should().Be("HR Department ITD004");

        var logs = await GetAuditLogsForEntityAsync(body.Data.Id.ToString());
        logs.Should()
            .ContainSingle(a => a.Action == "CREATE" && a.EntityType == "Department",
                "a CREATE audit log entry must be written after department creation");
    }

    // ── IT-D-005 / AC-27: Update → AuditLog entry ─────────────────────────────

    /// <summary>
    /// IT-D-005: PUT /api/v1/departments/{id} produces an AuditLog row with Action="UPDATE".
    /// </summary>
    [Fact]
    public async Task ITD005_UpdateDepartment_WritesAuditLogUpdateEntry()
    {
        var jwt = await SeedAdminAndGetJwtAsync("005");
        var dept = await SeedDepartmentAsync("Finance ITD005", "FINITD005");
        using var c = AuthedClient(jwt);

        var response = await c.PutAsJsonAsync($"/api/v1/departments/{dept.Id}", new
        {
            name = "Finance ITD005 Updated",
            overlapLimit = 4,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeptResponse>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Name.Should().Be("Finance ITD005 Updated");

        var logs = await GetAuditLogsForEntityAsync(dept.Id.ToString());
        logs.Should()
            .ContainSingle(a => a.Action == "UPDATE" && a.EntityType == "Department",
                "an UPDATE audit log entry must be written after department update");
    }

    // ── IT-D-006 / AC-27: Deactivate → AuditLog entry + soft-delete ───────────

    /// <summary>
    /// IT-D-006: DELETE /api/v1/departments/{id} soft-deactivates the department
    /// (sets DeletedAt, Status=Inactive) and produces an AuditLog row with Action="DEACTIVATE".
    /// </summary>
    [Fact]
    public async Task ITD006_DeactivateDepartment_WritesAuditLogAndSoftDeletes()
    {
        var jwt = await SeedAdminAndGetJwtAsync("006");
        var dept = await SeedDepartmentAsync("Legal ITD006", "LEGITD006");
        using var c = AuthedClient(jwt);

        var response = await c.DeleteAsync($"/api/v1/departments/{dept.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify AuditLog entry
        var logs = await GetAuditLogsForEntityAsync(dept.Id.ToString());
        logs.Should()
            .ContainSingle(a => a.Action == "DEACTIVATE" && a.EntityType == "Department",
                "a DEACTIVATE audit log entry must be written after department deactivation");

        // Verify soft-delete: DeletedAt is set and Status is Inactive
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
        var updated = await db.Departments.FindAsync(dept.Id);
        updated.Should().NotBeNull();
        updated!.DeletedAt.Should().NotBeNull("soft delete must set DeletedAt");
        updated.Status.Should().Be("Inactive");
    }

    // ── IT-D-007: GET /api/v1/departments → list with data ────────────────────

    /// <summary>
    /// IT-D-007: GET /api/v1/departments returns HTTP 200
    /// with ApiResponse{ data: [...] } — never bare array.
    /// </summary>
    [Fact]
    public async Task ITD007_GetDepartments_ReturnsApiResponseWithData()
    {
        var jwt = await SeedAdminAndGetJwtAsync("007");
        await SeedDepartmentAsync("Ops ITD007", "OPSITD007");
        using var c = AuthedClient(jwt);

        var response = await c.GetAsync("/api/v1/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠️ Assert ApiResponse<T>.Data exists — never .Items or .Result
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DeptResponse>>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Should().NotBeEmpty("at least one department was seeded");
    }

    // ── IT-D-008: POST creates dept with correct response shape ────────────────

    /// <summary>
    /// IT-D-008: POST /api/v1/departments with valid data returns HTTP 201
    /// and ApiResponse{ data: { id, name, code, overlapLimit, status="Active" } }.
    /// </summary>
    [Fact]
    public async Task ITD008_CreateDepartment_ValidData_Returns201WithCorrectShape()
    {
        var jwt = await SeedAdminAndGetJwtAsync("008");
        using var c = AuthedClient(jwt);

        var response = await c.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Sales ITD008",
            code = "SALITD008",
            overlapLimit = 5,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // ⚠️ Assert ApiResponse<T>.Data exists
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeptResponse>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.Data.Id.Should().NotBeEmpty();
        body.Data.Name.Should().Be("Sales ITD008");
        body.Data.Code.Should().Be("SALITD008");
        body.Data.OverlapLimit.Should().Be(5);
        body.Data.Status.Should().Be("Active");
    }
}

// ── xUnit collection fixture ────────────────────────────────────────────────────

[CollectionDefinition("DepartmentIntegration")]
public sealed class DepartmentIntegrationCollection
    : ICollectionFixture<LmsWebApplicationFactory>
{
}

// ── Local response DTO for deserialization ─────────────────────────────────────

internal sealed class DeptResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int OverlapLimit { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
