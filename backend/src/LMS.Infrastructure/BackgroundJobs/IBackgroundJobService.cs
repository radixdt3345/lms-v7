namespace LMS.Infrastructure.BackgroundJobs;

public interface IBackgroundJobService
{
    Task<string> ExpireCompOffCreditsAsync(CancellationToken ct = default);
    Task<string> ResetAnnualLeaveBalancesAsync(int year, CancellationToken ct = default);
    Task<string> SendPendingLeaveRemindersAsync(CancellationToken ct = default);
    Task<IEnumerable<JobLogDto>> GetRecentJobLogsAsync(int count = 20, CancellationToken ct = default);
}

public record JobLogDto(Guid Id, string JobName, string Status, string? Details, DateTime StartedAt, DateTime? CompletedAt);
