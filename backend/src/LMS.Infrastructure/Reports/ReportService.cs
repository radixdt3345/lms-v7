using System.Text;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Reports;

public sealed class ReportService : IReportService
{
    private readonly LmsDbContext _db;
    public ReportService(LmsDbContext db) => _db = db;

    public async Task<byte[]> GetLeaveReportCsvAsync(int year, Guid? employeeId = null)
    {
        var q = _db.LeaveApplications.Include(a => a.Employee).Include(a => a.LeaveType)
            .Where(a => a.StartDate.Year == year || a.EndDate.Year == year);
        if (employeeId.HasValue) q = q.Where(a => a.EmployeeId == employeeId);
        var data = await q.OrderBy(a => a.StartDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Employee,LeaveType,StartDate,EndDate,TotalDays,Status,Reason");
        foreach (var a in data)
            sb.AppendLine($""{a.Employee.Name}","{a.LeaveType.Name}",{a.StartDate},{a.EndDate},{a.TotalDays},{a.Status},"{a.Reason}"");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GetCompOffReportCsvAsync(int year, Guid? employeeId = null)
    {
        var q = _db.CompOffRequests.Include(r => r.Employee)
            .Where(r => r.WorkedDate.Year == year);
        if (employeeId.HasValue) q = q.Where(r => r.EmployeeId == employeeId);
        var data = await q.OrderBy(r => r.WorkedDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Employee,WorkedDate,CreditDays,Status,Reason");
        foreach (var r in data)
            sb.AppendLine($""{r.Employee.Name}",{r.WorkedDate},{r.CreditDays},{r.Status},"{r.Reason}"");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GetLeaveBalanceReportCsvAsync(int year)
    {
        var data = await _db.LeaveBalances.Include(b => b.Employee).Include(b => b.LeaveType)
            .Where(b => b.Year == year).OrderBy(b => b.Employee.Name).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Employee,LeaveType,Year,Total,Used,Pending,Available");
        foreach (var b in data)
            sb.AppendLine($""{b.Employee.Name}","{b.LeaveType.Name}",{b.Year},{b.TotalDays},{b.UsedDays},{b.PendingDays},{b.TotalDays - b.UsedDays - b.PendingDays}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
