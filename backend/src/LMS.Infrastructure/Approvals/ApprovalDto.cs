namespace LMS.Infrastructure.Approvals;

public sealed record PendingApprovalDto(
    Guid Id,
    string EntityType,
    string EmployeeName,
    string Description,
    string Status,
    DateTime SubmittedAt
);

public sealed record ApprovalHistoryDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string ActorName,
    string Action,
    string? Comments,
    DateTime ActedAt
);
