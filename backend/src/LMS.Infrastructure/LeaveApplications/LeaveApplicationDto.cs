namespace LMS.Infrastructure.LeaveApplications;

public sealed record LeaveApplicationDto(
    Guid Id, Guid EmployeeId, string EmployeeName,
    Guid LeaveTypeId, string LeaveTypeName,
    DateOnly StartDate, DateOnly EndDate, decimal TotalDays,
    string Reason, string Status,
    Guid? ApprovedById, string? ApprovedByName,
    DateTime? ApprovedAt, string? RejectionReason,
    DateTime CreatedAt);

public sealed record SubmitLeaveApplicationRequest(
    Guid LeaveTypeId, DateOnly StartDate, DateOnly EndDate, string Reason);

public sealed record ApproveLeaveApplicationRequest(string? Comments = null);
public sealed record RejectLeaveApplicationRequest(string RejectionReason);
