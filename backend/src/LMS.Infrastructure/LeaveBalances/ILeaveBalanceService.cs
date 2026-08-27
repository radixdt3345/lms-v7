namespace LMS.Infrastructure.LeaveBalances;

public interface ILeaveBalanceService
{
    Task<IReadOnlyList<LeaveBalanceDto>> GetMyBalancesAsync(Guid userId, int year);
    Task<IReadOnlyList<LeaveBalanceDto>> GetEmployeeBalancesAsync(Guid employeeId, int year);
    Task<IReadOnlyList<LeaveBalanceDto>> GetAllBalancesAsync(int year);
    Task CreditAnnualBalancesAsync(int year);
    Task AdjustBalanceAsync(AdjustBalanceRequest request);
}
