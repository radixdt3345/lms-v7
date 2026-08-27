namespace LMS.Infrastructure.LeaveTypes;

public sealed record LeaveTypeDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    int AnnualDays,
    bool RequiresAttachment,
    bool RequiresHrApproval,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateLeaveTypeRequest(
    string Name,
    string Code,
    string? Description,
    int AnnualDays,
    bool RequiresAttachment,
    bool RequiresHrApproval);

public sealed record UpdateLeaveTypeRequest(
    string? Name,
    string? Code,
    string? Description,
    int? AnnualDays,
    bool? RequiresAttachment,
    bool? RequiresHrApproval,
    bool? IsActive);
