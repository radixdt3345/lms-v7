using System.Text.Json;
using LMS.Infrastructure.AuditLogs;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Departments;

public sealed class DepartmentService : IDepartmentService
{
    private readonly LmsDbContext _db;
    private readonly IAuditLogService _audit;

    public DepartmentService(LmsDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    private static DepartmentDto ToDto(Department d) =>
        new(d.Id, d.Name, d.Code, d.OverlapLimit, d.Status, d.CreatedAt, d.UpdatedAt);

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Departments
            .Where(d => d.DeletedAt == null)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Code, d.OverlapLimit, d.Status, d.CreatedAt, d.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _db.Departments
            .Where(x => x.Id == id && x.DeletedAt == null)
            .FirstOrDefaultAsync(ct);
        return d is null ? null : ToDto(d);
    }

    public async Task<(DepartmentDto? Dept, string? Error)> CreateAsync(
        CreateDepartmentRequest request,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var nameToCheck = request.Name.Trim();
        var codeToCheck = request.Code.Trim().ToUpperInvariant();

        var duplicate = await _db.Departments.AnyAsync(
            d => d.DeletedAt == null
                && (EF.Functions.ILike(d.Name, nameToCheck)
                    || EF.Functions.ILike(d.Code, codeToCheck)),
            ct
        );

        if (duplicate)
            return (null, "DUPLICATE_DEPARTMENT_NAME");

        var dept = new Department
        {
            Name = nameToCheck,
            Code = codeToCheck,
            OverlapLimit = request.OverlapLimit,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "Department",
            dept.Id.ToString(),
            "CREATE",
            actorId,
            actorEmail,
            JsonSerializer.Serialize(new { dept.Name, dept.Code, dept.OverlapLimit }),
            ct
        );

        return (ToDto(dept), null);
    }

    public async Task<(DepartmentDto? Dept, string? Error)> UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var dept = await _db.Departments
            .Where(d => d.Id == id && d.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (dept is null)
            return (null, "NOT_FOUND");

        if (request.Name is not null)
        {
            var nameToCheck = request.Name.Trim();
            var duplicate = await _db.Departments.AnyAsync(
                d => d.Id != id && d.DeletedAt == null && EF.Functions.ILike(d.Name, nameToCheck),
                ct
            );
            if (duplicate)
                return (null, "DUPLICATE_DEPARTMENT_NAME");
            dept.Name = nameToCheck;
        }

        if (request.Code is not null)
        {
            var codeToCheck = request.Code.Trim().ToUpperInvariant();
            var duplicate = await _db.Departments.AnyAsync(
                d => d.Id != id && d.DeletedAt == null && EF.Functions.ILike(d.Code, codeToCheck),
                ct
            );
            if (duplicate)
                return (null, "DUPLICATE_DEPARTMENT_NAME");
            dept.Code = codeToCheck;
        }

        if (request.OverlapLimit.HasValue)
            dept.OverlapLimit = request.OverlapLimit.Value;

        if (request.Status is not null)
            dept.Status = request.Status;

        dept.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "Department",
            dept.Id.ToString(),
            "UPDATE",
            actorId,
            actorEmail,
            JsonSerializer.Serialize(new { request.Name, request.Code, request.OverlapLimit, request.Status }),
            ct
        );

        return (ToDto(dept), null);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(
        Guid id,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default)
    {
        var dept = await _db.Departments
            .Where(d => d.Id == id && d.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (dept is null)
            return (false, "NOT_FOUND");

        dept.Status = "Inactive";
        dept.DeletedAt = DateTime.UtcNow;
        dept.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "Department",
            dept.Id.ToString(),
            "DEACTIVATE",
            actorId,
            actorEmail,
            null,
            ct
        );

        return (true, null);
    }
}
