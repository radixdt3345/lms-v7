using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.BackgroundJobs;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly LmsDbContext _db;
    public BackgroundJobService(LmsDbContext db) => _db = db;

    public async Task<string> ExpireCompOffCreditsAsync(CancellationToken ct = default)
    {
        var log = await StartJobAsync("ExpireCompOffCredits", null, ct);
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var expired = await _db.CompOffCredits
                .Where(c => c.Status == "Active" && c.ExpiryDate < today)
                .ToListAsync(ct);
            foreach (var c in expired) c.Status = "Expired";
            await _db.SaveChangesAsync(ct);
            var msg = $"{expired.Count} comp-off credits expired.";
            await CompleteJobAsync(log, "Success", msg, ct);
            return msg;
        }
        catch (Exception ex)
        {
            await CompleteJobAsync(log, "Failed", ex.Message, ct);
            throw;
        }
    }

    public async Task<string> ResetAnnualLeaveBalancesAsync(int year, CancellationToken ct = default)
    {
        var log = await StartJobAsync("ResetAnnualLeaveBalances", null, ct);
        try
        {
            var leaveTypes = await _db.LeaveTypes.Where(lt => lt.IsActive).ToListAsync(ct);
            var employees = await _db.Users.Where(u => u.Status == "Active" && u.Role == "EMPLOYEE").ToListAsync(ct);
            int created = 0;
            foreach (var emp in employees)
            {
                foreach (var lt in leaveTypes)
                {
                    var exists = await _db.LeaveBalances.AnyAsync(
                        b => b.EmployeeId == emp.Id && b.LeaveTypeId == lt.Id && b.Year == year, ct);
                    if (!exists)
                    {
                        _db.LeaveBalances.Add(new LeaveBalance
                        {
                            EmployeeId = emp.Id,
                            LeaveTypeId = lt.Id,
                            Year = year,
                            TotalDays = lt.DefaultDays,
                            UsedDays = 0,
                            PendingDays = 0
                        });
                        created++;
                    }
                }
            }
            await _db.SaveChangesAsync(ct);
            var msg = $"{created} leave balances created for {year}.";
            await CompleteJobAsync(log, "Success", msg, ct);
            return msg;
        }
        catch (Exception ex)
        {
            await CompleteJobAsync(log, "Failed", ex.Message, ct);
            throw;
        }
    }

    public async Task<string> SendPendingLeaveRemindersAsync(CancellationToken ct = default)
    {
        var log = await StartJobAsync("SendPendingLeaveReminders", null, ct);
        try
        {
            var pending = await _db.LeaveApplications
                .Where(l => l.Status == "Pending")
                .CountAsync(ct);
            // In production, this would send emails/notifications
            var msg = $"{pending} pending leave applications require review.";
            await CompleteJobAsync(log, "Success", msg, ct);
            return msg;
        }
        catch (Exception ex)
        {
            await CompleteJobAsync(log, "Failed", ex.Message, ct);
            throw;
        }
    }

    public async Task<IEnumerable<JobLogDto>> GetRecentJobLogsAsync(int count = 20, CancellationToken ct = default)
    {
        var logs = await _db.ScheduledJobLogs
            .OrderByDescending(l => l.StartedAt)
            .Take(count)
            .ToListAsync(ct);
        return logs.Select(l => new JobLogDto(l.Id, l.JobName, l.Status, l.Details, l.StartedAt, l.CompletedAt));
    }

    private async Task<ScheduledJobLog> StartJobAsync(string name, Guid? triggeredBy, CancellationToken ct)
    {
        var log = new ScheduledJobLog { JobName = name, Status = "Running", StartedAt = DateTime.UtcNow, TriggeredBy = triggeredBy };
        _db.ScheduledJobLogs.Add(log);
        await _db.SaveChangesAsync(ct);
        return log;
    }

    private async Task CompleteJobAsync(ScheduledJobLog log, string status, string? details, CancellationToken ct)
    {
        log.Status = status;
        log.Details = details;
        log.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
