using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Approvals;

public sealed class ApprovalService : IApprovalService
{
    private readonly LmsDbContext _db;
    public ApprovalService(LmsDbContext db) => _db = db;

    public async Task<List<PendingApprovalDto>> GetPendingApprovalsAsync()
    {
        var leaves = await _db.LeaveApplications
            .Include(a => a.Employee)
            .Where(a => a.Status == "Pending")
            .Select(a => new PendingApprovalDto(
                a.Id, "LeaveApplication", a.Employee.Name,
                $"{a.LeaveType.Name}: {a.StartDate} - {a.EndDate} ({a.TotalDays}d)",
                a.Status, a.CreatedAt))
            .ToListAsync();

        var compOff = await _db.CompOffRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == "Pending")
            .Select(r => new PendingApprovalDto(
                r.Id, "CompOffRequest", r.Employee.Name,
                $"Comp-Off: {r.WorkedDate} ({r.CreditDays}d) - {r.Reason}",
                r.Status, r.CreatedAt))
            .ToListAsync();

        return leaves.Concat(compOff).OrderBy(a => a.SubmittedAt).ToList();
    }

    public async Task<List<ApprovalHistoryDto>> GetApprovalHistoryAsync(string entityType, Guid entityId)
    {
        return await _db.ApprovalHistories
            .Include(h => h.Actor)
            .Where(h => h.EntityType == entityType && h.EntityId == entityId)
            .OrderByDescending(h => h.ActedAt)
            .Select(h => new ApprovalHistoryDto(
                h.Id, h.EntityType, h.EntityId, h.Actor.Name, h.Action, h.Comments, h.ActedAt))
            .ToListAsync();
    }
}
