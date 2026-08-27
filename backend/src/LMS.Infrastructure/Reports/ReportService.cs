using System.Text;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Reports;

public class ReportService : IReportService
{
    private readonly LmsDbContext _db;
    public ReportService(LmsDbContext db) => _db = db;

    public async Task<string> GenerateLeaveReportAsync(int? year = null, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var rows = await _db.LeaveApplications
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => l.StartDate.Year == y)
            .OrderBy(l => l.StartDate)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeName,Email,LeaveType,StartDate,EndDate,TotalDays,Status,Reason");
        foreach (var r in rows)
            sb.AppendLine($""{r.Employee.Name}","{r.Employee.Email}","{r.LeaveType.Name}",{r.StartDate:yyyy-MM-dd},{r.EndDate:yyyy-MM-dd},{r.TotalDays},{r.Status},"{r.Reason?.Replace(""","""")}"");
        return sb.ToString();
    }

    public async Task<string> GenerateCompOffReportAsync(int? year = null, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var rows = await _db.CompOffRequests
            .Include(c => c.Employee)
            .Where(c => c.WorkedDate.Year == y)
            .OrderBy(c => c.WorkedDate)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeName,Email,WorkedDate,CreditDays,Status,Reason");
        foreach (var r in rows)
            sb.AppendLine($""{r.Employee.Name}","{r.Employee.Email}",{r.WorkedDate:yyyy-MM-dd},{r.CreditDays},{r.Status},"{r.Reason?.Replace(""","""")}"");
        return sb.ToString();
    }

    public async Task<string> GenerateLeaveBalanceReportAsync(int? year = null, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var rows = await _db.LeaveBalances
            .Include(b => b.Employee)
            .Include(b => b.LeaveType)
            .Where(b => b.Year == y)
            .OrderBy(b => b.Employee.Name)
            .ThenBy(b => b.LeaveType.Name)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("EmployeeName,Email,LeaveType,Year,TotalDays,UsedDays,PendingDays,AvailableDays");
        foreach (var r in rows)
            sb.AppendLine($""{r.Employee.Name}","{r.Employee.Email}","{r.LeaveType.Name}",{r.Year},{r.TotalDays},{r.UsedDays},{r.PendingDays},{r.TotalDays - r.UsedDays - r.PendingDays}");
        return sb.ToString();
    }
}
