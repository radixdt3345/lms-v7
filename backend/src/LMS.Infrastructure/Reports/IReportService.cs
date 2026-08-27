using LMS.Infrastructure.Reports;

namespace LMS.Infrastructure.Reports;

public interface IReportService
{
    Task<string> GenerateLeaveReportAsync(int? year = null, CancellationToken ct = default);
    Task<string> GenerateCompOffReportAsync(int? year = null, CancellationToken ct = default);
    Task<string> GenerateLeaveBalanceReportAsync(int? year = null, CancellationToken ct = default);
}
