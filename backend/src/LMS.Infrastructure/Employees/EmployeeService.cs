using System.Text.Json;
using LMS.Infrastructure.AuditLogs;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Employees;

public sealed class EmployeeService : IEmployeeService
{
    private readonly LmsDbContext _db;
    private readonly IAuditLogService _audit;

    public EmployeeService(LmsDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    // ── Projection helper ────────────────────────────────────────────────────

    private static EmployeeDto ToDto(User u) =>
        new(
            u.Id, u.Name, u.Email, u.Phone, u.Role, u.Status,
            u.JobTitle, u.DateOfJoining,
            u.DepartmentId, u.Department?.Name,
            u.ReportingManagerId, u.ReportingManager?.Name,
            u.CreatedAt, u.UpdatedAt
        );

    private async Task<EmployeeDto> LoadDtoAsync(Guid id, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.ReportingManager)
            .FirstAsync(u => u.Id == id, ct);
        return ToDto(user);
    }

    // ── Auto-promote: if manager-designate is EMPLOYEE, upgrade them ─────────

    private async Task AutoPromoteIfNeededAsync(Guid managerId, DateTime now, CancellationToken ct)
    {
        var manager = await _db.Users.FindAsync([managerId], ct);
        if (manager is not null && manager.Role == "EMPLOYEE")
        {
            manager.Role = "MANAGER";
            manager.UpdatedAt = now;
        }
    }

    // ── Auto-demote: if former manager has zero remaining direct reports ──────

    private async Task AutoDemoteIfNeededAsync(Guid managerId, DateTime now, CancellationToken ct)
    {
        var hasReports = await _db.Users
            .AnyAsync(u => u.ReportingManagerId == managerId && u.DeletedAt == null, ct);
        if (!hasReports)
        {
            var manager = await _db.Users.FindAsync([managerId], ct);
            if (manager is not null && manager.Role == "MANAGER")
            {
                manager.Role = "EMPLOYEE";
                manager.UpdatedAt = now;
            }
        }
    }

    // ── List ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EmployeeDto>> ListAsync(CancellationToken ct = default)
    {
        return await _db.Users
            .Include(u => u.Department)
            .Include(u => u.ReportingManager)
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.Name)
            .Select(u => new EmployeeDto(
                u.Id, u.Name, u.Email, u.Phone, u.Role, u.Status,
                u.JobTitle, u.DateOfJoining,
                u.DepartmentId, u.Department != null ? u.Department.Name : null,
                u.ReportingManagerId, u.ReportingManager != null ? u.ReportingManager.Name : null,
                u.CreatedAt, u.UpdatedAt))
            .ToListAsync(ct);
    }

    // ── Get by ID (no DeletedAt guard — AC-19: still retrievable after deactivation) ─

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.ReportingManager)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        return user is null ? null : ToDto(user);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    public async Task<(EmployeeDto? Employee, string? Error)> CreateAsync(
        CreateEmployeeRequest req,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == email && u.DeletedAt == null, ct);
        if (emailExists)
            return (null, "DUPLICATE_EMAIL");

        var now = DateTime.UtcNow;
        var employee = new User
        {
            Name = req.Name.Trim(),
            Email = email,
            Phone = req.Phone?.Trim(),
            JobTitle = req.JobTitle?.Trim(),
            DateOfJoining = req.DateOfJoining,
            DepartmentId = req.DepartmentId,
            ReportingManagerId = req.ReportingManagerId,
            Role = "EMPLOYEE",
            Status = "Active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Users.Add(employee);

        // AC-20: auto-promote reporting manager if they are EMPLOYEE
        if (req.ReportingManagerId.HasValue)
            await AutoPromoteIfNeededAsync(req.ReportingManagerId.Value, now, ct);

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "User", employee.Id.ToString(), "CREATE", actorId, actorEmail,
            JsonSerializer.Serialize(new { employee.Name, employee.Email, employee.Role }), ct);

        return (await LoadDtoAsync(employee.Id, ct), null);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    public async Task<(EmployeeDto? Employee, string? Error)> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest req,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var employee = await _db.Users.FindAsync([id], ct);
        if (employee is null)
            return (null, "NOT_FOUND");

        var now = DateTime.UtcNow;

        // AC-22: block manual demotion when active reports exist
        if (req.Role is "EMPLOYEE" && employee.Role == "MANAGER")
        {
            var hasActiveReports = await _db.Users
                .AnyAsync(u => u.ReportingManagerId == id && u.DeletedAt == null, ct);
            if (hasActiveReports)
                return (null, "MANAGER_HAS_ACTIVE_REPORTS");
        }

        var oldManagerId = employee.ReportingManagerId;

        if (req.Name is not null) employee.Name = req.Name.Trim();
        if (req.Phone is not null) employee.Phone = req.Phone.Trim();
        if (req.JobTitle is not null) employee.JobTitle = req.JobTitle.Trim();
        if (req.DateOfJoining.HasValue) employee.DateOfJoining = req.DateOfJoining;
        if (req.Role is not null) employee.Role = req.Role;
        if (req.Status is not null) employee.Status = req.Status;

        if (req.ClearDepartment)
            employee.DepartmentId = null;
        else if (req.DepartmentId.HasValue)
            employee.DepartmentId = req.DepartmentId;

        Guid? newManagerId = null;
        if (req.ClearReportingManager)
        {
            employee.ReportingManagerId = null;
        }
        else if (req.ReportingManagerId.HasValue)
        {
            newManagerId = req.ReportingManagerId.Value;
            employee.ReportingManagerId = newManagerId;
        }

        employee.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        // AC-20: auto-promote new manager if EMPLOYEE
        if (newManagerId.HasValue)
            await AutoPromoteIfNeededAsync(newManagerId.Value, now, ct);

        // AC-21: auto-demote old manager if they now have zero direct reports
        if (oldManagerId.HasValue && oldManagerId != newManagerId)
            await AutoDemoteIfNeededAsync(oldManagerId.Value, now, ct);

        if (newManagerId.HasValue || (oldManagerId.HasValue && oldManagerId != newManagerId))
            await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "User", id.ToString(), "UPDATE", actorId, actorEmail,
            JsonSerializer.Serialize(new { req.Name, req.Role, req.Status }), ct);

        return (await LoadDtoAsync(id, ct), null);
    }

    // ── Deactivate (soft delete) ──────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> DeactivateAsync(
        Guid id,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var employee = await _db.Users.FindAsync([id], ct);
        if (employee is null || employee.DeletedAt != null)
            return (false, "NOT_FOUND");

        var now = DateTime.UtcNow;
        var oldManagerId = employee.ReportingManagerId;

        employee.Status = "Inactive";
        employee.DeletedAt = now;
        employee.UpdatedAt = now;
        // Nullify reporting manager — removes this report from manager's count
        employee.ReportingManagerId = null;

        // Also nullify this employee as manager for their direct reports
        await _db.Users
            .Where(u => u.ReportingManagerId == id && u.DeletedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.ReportingManagerId, (Guid?)null)
                .SetProperty(u => u.UpdatedAt, now), ct);

        await _db.SaveChangesAsync(ct);

        // AC-21: auto-demote old manager if no more reports
        if (oldManagerId.HasValue)
        {
            await AutoDemoteIfNeededAsync(oldManagerId.Value, now, ct);
            await _db.SaveChangesAsync(ct);
        }

        await _audit.LogAsync(
            "User", id.ToString(), "DEACTIVATE", actorId, actorEmail, null, ct);

        return (true, null);
    }

    // ── Me ───────────────────────────────────────────────────────────────────

    public async Task<EmployeeDto?> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.ReportingManager)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        return user is null ? null : ToDto(user);
    }

    // ── Self-edit (AC-23: name + phone only) ─────────────────────────────────

    public async Task<(EmployeeDto? Employee, string? Error)> SelfEditAsync(
        Guid userId,
        SelfEditRequest req,
        CancellationToken ct = default)
    {
        var employee = await _db.Users.FindAsync([userId], ct);
        if (employee is null || employee.DeletedAt != null)
            return (null, "NOT_FOUND");

        employee.Name = req.Name.Trim();
        employee.Phone = req.Phone?.Trim();
        employee.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return (await LoadDtoAsync(userId, ct), null);
    }

    // ── Team (AC-24: direct reports) ──────────────────────────────────────────

    public async Task<IReadOnlyList<EmployeeDto>> GetTeamAsync(
        Guid managerId,
        CancellationToken ct = default)
    {
        return await _db.Users
            .Include(u => u.Department)
            .Include(u => u.ReportingManager)
            .Where(u => u.ReportingManagerId == managerId && u.DeletedAt == null)
            .OrderBy(u => u.Name)
            .Select(u => new EmployeeDto(
                u.Id, u.Name, u.Email, u.Phone, u.Role, u.Status,
                u.JobTitle, u.DateOfJoining,
                u.DepartmentId, u.Department != null ? u.Department.Name : null,
                u.ReportingManagerId, u.ReportingManager != null ? u.ReportingManager.Name : null,
                u.CreatedAt, u.UpdatedAt))
            .ToListAsync(ct);
    }

    // ── Anonymise (GDPR Art.17) ───────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> AnonymiseAsync(
        Guid id,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var employee = await _db.Users.FindAsync([id], ct);
        if (employee is null)
            return (false, "NOT_FOUND");

        var now = DateTime.UtcNow;
        employee.Name = "[deleted]";
        employee.Email = $"[deleted-{id}]";
        employee.Phone = null;
        employee.JobTitle = null;
        employee.DateOfJoining = null;
        employee.PasswordHash = null;
        employee.Status = "Inactive";
        employee.DeletedAt ??= now;
        employee.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "User", id.ToString(), "ANONYMISE", actorId, actorEmail, null, ct);

        return (true, null);
    }
}
