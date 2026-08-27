namespace LMS.Infrastructure.Dashboard;
public interface IDashboardService
{
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(Guid employeeId);
    Task<HrDashboardDto> GetHrDashboardAsync();
}
