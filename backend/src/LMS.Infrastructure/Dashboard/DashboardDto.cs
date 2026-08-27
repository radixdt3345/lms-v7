namespace LMS.Infrastructure.Dashboard;

public sealed record EmployeeDashboardDto(
    int PendingLeaves, int ApprovedLeaves, int RejectedLeaves,
    int PendingCompOff, decimal TotalLeaveBalance, int UnreadNotifications,
    List<LeaveBalanceSummary> LeaveBalances);

public sealed record LeaveBalanceSummary(string LeaveTypeName, decimal Total, decimal Used, decimal Remaining);

public sealed record HrDashboardDto(
    int TotalEmployees, int PendingLeaveApprovals, int PendingCompOffApprovals,
    int TodayOnLeave, int ThisMonthApprovals);
