using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.LeaveTypes;

public sealed class LeaveTypeService : ILeaveTypeService
{
    private readonly LmsDbContext _db;

    public LeaveTypeService(LmsDbContext db)
    {
        _db = db;
    }

    private static LeaveTypeDto ToDto(LeaveType lt) =>
        new(lt.Id, lt.Name, lt.Code, lt.Description, lt.AnnualDays,
            lt.RequiresAttachment, lt.RequiresHrApproval, lt.IsActive,
            lt.CreatedAt, lt.UpdatedAt);

    public async Task<IReadOnlyList<LeaveTypeDto>> ListAsync(CancellationToken ct = default)
    {
        return await _db.LeaveTypes
            .OrderBy(lt => lt.Name)
            .Select(lt => new LeaveTypeDto(
                lt.Id, lt.Name, lt.Code, lt.Description, lt.AnnualDays,
                lt.RequiresAttachment, lt.RequiresHrApproval, lt.IsActive,
                lt.CreatedAt, lt.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<LeaveTypeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var lt = await _db.LeaveTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
        return lt is null ? null : ToDto(lt);
    }

    public async Task<(LeaveTypeDto? Dto, string? Error)> CreateAsync(
        CreateLeaveTypeRequest req,
        CancellationToken ct = default)
    {
        var nameTrimmed = req.Name.Trim();
        var codeTrimmed = req.Code.Trim().ToUpperInvariant();

        var duplicate = await _db.LeaveTypes.AnyAsync(
            lt => EF.Functions.ILike(lt.Name, nameTrimmed)
                || EF.Functions.ILike(lt.Code, codeTrimmed),
            ct);

        if (duplicate)
            return (null, "DUPLICATE_LEAVE_TYPE");

        var lt = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = nameTrimmed,
            Code = codeTrimmed,
            Description = req.Description?.Trim(),
            AnnualDays = req.AnnualDays,
            RequiresAttachment = req.RequiresAttachment,
            RequiresHrApproval = req.RequiresHrApproval,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.LeaveTypes.Add(lt);
        await _db.SaveChangesAsync(ct);
        return (ToDto(lt), null);
    }

    public async Task<(LeaveTypeDto? Dto, string? Error)> UpdateAsync(
        Guid id,
        UpdateLeaveTypeRequest req,
        CancellationToken ct = default)
    {
        var lt = await _db.LeaveTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (lt is null)
            return (null, "NOT_FOUND");

        if (req.Name is not null)
        {
            var newName = req.Name.Trim();
            var nameTaken = await _db.LeaveTypes.AnyAsync(
                x => x.Id != id && EF.Functions.ILike(x.Name, newName), ct);
            if (nameTaken)
                return (null, "DUPLICATE_LEAVE_TYPE");
            lt.Name = newName;
        }

        if (req.Code is not null)
        {
            var newCode = req.Code.Trim().ToUpperInvariant();
            var codeTaken = await _db.LeaveTypes.AnyAsync(
                x => x.Id != id && EF.Functions.ILike(x.Code, newCode), ct);
            if (codeTaken)
                return (null, "DUPLICATE_LEAVE_TYPE");
            lt.Code = newCode;
        }

        if (req.Description is not null) lt.Description = req.Description.Trim();
        if (req.AnnualDays.HasValue) lt.AnnualDays = req.AnnualDays.Value;
        if (req.RequiresAttachment.HasValue) lt.RequiresAttachment = req.RequiresAttachment.Value;
        if (req.RequiresHrApproval.HasValue) lt.RequiresHrApproval = req.RequiresHrApproval.Value;
        if (req.IsActive.HasValue) lt.IsActive = req.IsActive.Value;

        lt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return (ToDto(lt), null);
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var lt = await _db.LeaveTypes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (lt is null) return false;
        lt.IsActive = false;
        lt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
