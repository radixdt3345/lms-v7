using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly LmsDbContext _db;
    public DashboardService(LmsDbContext db) => _db = db;

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(Guid employeeId)
    {
        var year = DateTime.UtcNow.Year;
        var leaves = await _db.LeaveApplications.Where(a => a.EmployeeId == employeeId).ToListAsync();
        var balances = await _db.LeaveBalances
            .Include(b => b.LeaveType)
            .Where(b => b.EmployeeId == employeeId && b.Year == year)
            .ToListAsync();
        var compOff = await _db.CompOffRequests.Where(r => r.EmployeeId == employeeId).ToListAsync();
        var unread = await _db.Notifications.CountAsync(n => n.UserId == employeeId && !n.IsRead);

        return new EmployeeDashboardDto(
            leaves.Count(a => a.Status == "Pending"),
            leaves.Count(a => a.Status == "Approved"),
            leaves.Count(a => a.Status == "Rejected"),
            compOff.Count(r => r.Status == "Pending"),
            balances.Sum(b => b.TotalDays - b.UsedDays),
            unread,
            balances.Select(b => new LeaveBalanceSummary(b.LeaveType.Name, b.TotalDays, b.UsedDays, b.TotalDays - b.UsedDays)).ToList()
        );
    }

    public async Task<HrDashboardDto> GetHrDashboardAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        return new HrDashboardDto(
            await _db.Users.CountAsync(u => u.Status == "Active"),
            await _db.LeaveApplications.CountAsync(a => a.Status == "Pending"),
            await _db.CompOffRequests.CountAsync(r => r.Status == "Pending"),
            await _db.LeaveApplications.CountAsync(a => a.Status == "Approved" && a.StartDate <= today && a.EndDate >= today),
            await _db.LeaveApplications.CountAsync(a => a.Status == "Approved" && a.StartDate >= monthStart)
        );
    }
}
