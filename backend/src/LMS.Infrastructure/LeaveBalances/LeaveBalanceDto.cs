namespace LMS.Infrastructure.LeaveBalances;

public sealed record LeaveBalanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveTypeId,
    string LeaveTypeName,
    int Year,
    decimal TotalDays,
    decimal UsedDays,
    decimal PendingDays,
    decimal RemainingDays
);

public sealed record AdjustBalanceRequest(
    Guid EmployeeId,
    Guid LeaveTypeId,
    int Year,
    decimal AdjustmentDays,
    string Reason
);

public sealed record CreditAnnualRequest(int Year);
