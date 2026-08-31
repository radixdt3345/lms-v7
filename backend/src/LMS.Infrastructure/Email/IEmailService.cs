namespace LMS.Infrastructure.Email;

public interface IEmailService
{
    Task SendLeaveAppliedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate);
    Task SendLeaveApprovedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate);
    Task SendLeaveRejectedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate, string? reason);
    Task SendLeaveCancelledAsync(string toEmail, string toName, string leaveType, string startDate, string endDate);
    Task SendLeaveRevokedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate);
    Task SendApprovalReminderAsync(string toEmail, string toName, int pendingCount);
    Task SendCompOffSubmittedAsync(string toEmail, string toName, string workedDate, decimal creditDays);
    Task SendCompOffApprovedAsync(string toEmail, string toName, string workedDate, decimal creditDays);
    Task SendCompOffRejectedAsync(string toEmail, string toName, string workedDate, string? reason);
}
