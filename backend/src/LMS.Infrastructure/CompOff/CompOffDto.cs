namespace LMS.Infrastructure.CompOff;

public sealed record CompOffRequestDto(
    Guid Id, Guid EmployeeId, string EmployeeName,
    DateOnly WorkedDate, decimal CreditDays, string Reason,
    string Status, Guid? ApprovedById, string? ApprovedByName,
    DateTime? ApprovedAt, string? RejectionReason, DateTime CreatedAt);

public sealed record SubmitCompOffRequest(DateOnly WorkedDate, decimal CreditDays, string Reason);
public sealed record RejectCompOffRequest(string RejectionReason);

public sealed record CompOffCreditDto(
    Guid Id, Guid EmployeeId, string EmployeeName,
    DateOnly EarnedDate, DateOnly ExpiryDate,
    decimal CreditDays, string Status);
