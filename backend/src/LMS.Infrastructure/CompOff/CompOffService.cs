using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.CompOff;

public sealed class CompOffService : ICompOffService
{
    private readonly LmsDbContext _db;
    public CompOffService(LmsDbContext db) => _db = db;

    private static CompOffRequestDto ToDto(CompOffRequest r) => new(
        r.Id, r.EmployeeId, r.Employee.Name,
        r.WorkedDate, r.CreditDays, r.Reason,
        r.Status, r.ApprovedById, r.ApprovedBy?.Name,
        r.ApprovedAt, r.RejectionReason, r.CreatedAt);

    public async Task<IReadOnlyList<CompOffRequestDto>> GetMyRequestsAsync(Guid userId) =>
        await _db.CompOffRequests.Include(r => r.Employee).Include(r => r.ApprovedBy)
            .Where(r => r.EmployeeId == userId).OrderByDescending(r => r.CreatedAt)
            .Select(r => ToDto(r)).ToListAsync();

    public async Task<IReadOnlyList<CompOffRequestDto>> GetAllRequestsAsync(string? status = null)
    {
        var q = _db.CompOffRequests.Include(r => r.Employee).Include(r => r.ApprovedBy).AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
        return await q.OrderByDescending(r => r.CreatedAt).Select(r => ToDto(r)).ToListAsync();
    }

    public async Task<CompOffRequestDto> SubmitRequestAsync(Guid employeeId, SubmitCompOffRequest request)
    {
        var req = new CompOffRequest
        {
            EmployeeId = employeeId, WorkedDate = request.WorkedDate,
            CreditDays = request.CreditDays, Reason = request.Reason,
            Status = "Pending", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.CompOffRequests.Add(req);
        await _db.SaveChangesAsync();
        return await _db.CompOffRequests.Include(r => r.Employee).Include(r => r.ApprovedBy)
            .Where(r => r.Id == req.Id).Select(r => ToDto(r)).FirstAsync();
    }

    public async Task<CompOffRequestDto> ApproveRequestAsync(Guid id, Guid approverId)
    {
        var req = await _db.CompOffRequests.FindAsync(id) ?? throw new KeyNotFoundException();
        req.Status = "Approved"; req.ApprovedById = approverId;
        req.ApprovedAt = DateTime.UtcNow; req.UpdatedAt = DateTime.UtcNow;
        // Grant comp-off credit
        _db.CompOffCredits.Add(new CompOffCredit
        {
            EmployeeId = req.EmployeeId, EarnedDate = req.WorkedDate,
            ExpiryDate = req.WorkedDate.AddMonths(3), CreditDays = req.CreditDays,
            Status = "Active", CompOffRequestId = req.Id,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return await _db.CompOffRequests.Include(r => r.Employee).Include(r => r.ApprovedBy)
            .Where(r => r.Id == id).Select(r => ToDto(r)).FirstAsync();
    }

    public async Task<CompOffRequestDto> RejectRequestAsync(Guid id, Guid rejectedById, string reason)
    {
        var req = await _db.CompOffRequests.FindAsync(id) ?? throw new KeyNotFoundException();
        req.Status = "Rejected"; req.ApprovedById = rejectedById;
        req.ApprovedAt = DateTime.UtcNow; req.RejectionReason = reason; req.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await _db.CompOffRequests.Include(r => r.Employee).Include(r => r.ApprovedBy)
            .Where(r => r.Id == id).Select(r => ToDto(r)).FirstAsync();
    }

    public async Task<IReadOnlyList<CompOffCreditDto>> GetMyCreditsAsync(Guid userId) =>
        await _db.CompOffCredits.Include(c => c.Employee)
            .Where(c => c.EmployeeId == userId)
            .Select(c => new CompOffCreditDto(c.Id, c.EmployeeId, c.Employee.Name,
                c.EarnedDate, c.ExpiryDate, c.CreditDays, c.Status))
            .ToListAsync();
}
