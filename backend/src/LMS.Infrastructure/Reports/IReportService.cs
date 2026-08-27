namespace LMS.Infrastructure.Reports;
public interface IReportService
{
    Task<byte[]> GetLeaveReportCsvAsync(int year, Guid? employeeId = null);
    Task<byte[]> GetCompOffReportCsvAsync(int year, Guid? employeeId = null);
    Task<byte[]> GetLeaveBalanceReportCsvAsync(int year);
}
