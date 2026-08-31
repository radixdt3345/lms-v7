using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly string _fromName;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _logger = logger;
        _host     = config["Smtp:Host"]     ?? "smtp.gmail.com";
        _port     = int.Parse(config["Smtp:Port"] ?? "587");
        _user     = config["Smtp:User"]     ?? throw new InvalidOperationException("Smtp:User is required.");
        _password = config["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password is required.");
        _fromName = config["Smtp:FromName"] ?? "LMS System";
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_user, _password),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_user, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            // Fire-and-forget: swallow so email failure never breaks the main flow
        }
    }

    private static string Layout(string heading, string body) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:24px;">
          <h2 style="color:#1F3864;border-bottom:2px solid #BDD7EE;padding-bottom:8px;">{heading}</h2>
          {body}
          <hr style="margin-top:32px;border:none;border-top:1px solid #ddd;" />
          <p style="color:#888;font-size:12px;">This is an automated message from the LMS System. Please do not reply.</p>
        </div>
        """;

    public Task SendLeaveAppliedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate) =>
        SendAsync(toEmail, toName, $"Leave Application Submitted — {leaveType}",
            Layout("Leave Application Submitted",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your leave application has been submitted and is pending approval.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Leave Type</td><td style='padding:6px 12px'>{leaveType}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>From</td><td style='padding:6px 12px'>{startDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold'>To</td><td style='padding:6px 12px;background:#f5f5f5'>{endDate}</td></tr>" +
                $"</table>"));

    public Task SendLeaveApprovedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate) =>
        SendAsync(toEmail, toName, $"Leave Approved — {leaveType}",
            Layout("✅ Leave Approved",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your leave application has been <strong style='color:green'>approved</strong>.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Leave Type</td><td style='padding:6px 12px'>{leaveType}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>From</td><td style='padding:6px 12px'>{startDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold'>To</td><td style='padding:6px 12px;background:#f5f5f5'>{endDate}</td></tr>" +
                $"</table>"));

    public Task SendLeaveRejectedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate, string? reason) =>
        SendAsync(toEmail, toName, $"Leave Rejected — {leaveType}",
            Layout("❌ Leave Rejected",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your leave application has been <strong style='color:red'>rejected</strong>.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Leave Type</td><td style='padding:6px 12px'>{leaveType}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>From</td><td style='padding:6px 12px'>{startDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold'>To</td><td style='padding:6px 12px;background:#f5f5f5'>{endDate}</td></tr>" +
                (reason != null ? $"<tr><td style='padding:6px 12px;font-weight:bold'>Reason</td><td style='padding:6px 12px'>{reason}</td></tr>" : "") +
                $"</table>"));

    public Task SendLeaveCancelledAsync(string toEmail, string toName, string leaveType, string startDate, string endDate) =>
        SendAsync(toEmail, toName, $"Leave Cancelled — {leaveType}",
            Layout("Leave Cancelled",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your leave application has been <strong>cancelled</strong>.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Leave Type</td><td style='padding:6px 12px'>{leaveType}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>From</td><td style='padding:6px 12px'>{startDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold'>To</td><td style='padding:6px 12px;background:#f5f5f5'>{endDate}</td></tr>" +
                $"</table>"));

    public Task SendLeaveRevokedAsync(string toEmail, string toName, string leaveType, string startDate, string endDate) =>
        SendAsync(toEmail, toName, $"Leave Revoked — {leaveType}",
            Layout("Leave Revoked",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your previously approved leave has been <strong style='color:orange'>revoked</strong> by HR Admin.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Leave Type</td><td style='padding:6px 12px'>{leaveType}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>From</td><td style='padding:6px 12px'>{startDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold'>To</td><td style='padding:6px 12px;background:#f5f5f5'>{endDate}</td></tr>" +
                $"</table>"));

    public Task SendApprovalReminderAsync(string toEmail, string toName, int pendingCount) =>
        SendAsync(toEmail, toName, $"Reminder: {pendingCount} Leave Request(s) Awaiting Your Approval",
            Layout("⏰ Pending Approvals Reminder",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>You have <strong>{pendingCount}</strong> leave request(s) awaiting your approval.</p>" +
                $"<p>Please log in to the LMS to review and take action.</p>"));

    public Task SendCompOffSubmittedAsync(string toEmail, string toName, string workedDate, decimal creditDays) =>
        SendAsync(toEmail, toName, "Comp-Off Request Submitted",
            Layout("Comp-Off Request Submitted",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your comp-off request has been submitted and is pending approval.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Worked Date</td><td style='padding:6px 12px'>{workedDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>Credit Days</td><td style='padding:6px 12px'>{creditDays}</td></tr>" +
                $"</table>"));

    public Task SendCompOffApprovedAsync(string toEmail, string toName, string workedDate, decimal creditDays) =>
        SendAsync(toEmail, toName, "Comp-Off Request Approved",
            Layout("✅ Comp-Off Approved",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your comp-off request has been <strong style='color:green'>approved</strong>. Credits have been added to your account.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Worked Date</td><td style='padding:6px 12px'>{workedDate}</td></tr>" +
                $"<tr><td style='padding:6px 12px;font-weight:bold'>Credit Days</td><td style='padding:6px 12px'>{creditDays}</td></tr>" +
                $"</table>"));

    public Task SendCompOffRejectedAsync(string toEmail, string toName, string workedDate, string? reason) =>
        SendAsync(toEmail, toName, "Comp-Off Request Rejected",
            Layout("❌ Comp-Off Rejected",
                $"<p>Hi <strong>{toName}</strong>,</p>" +
                $"<p>Your comp-off request has been <strong style='color:red'>rejected</strong>.</p>" +
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:6px 12px;background:#f5f5f5;font-weight:bold;width:140px'>Worked Date</td><td style='padding:6px 12px'>{workedDate}</td></tr>" +
                (reason != null ? $"<tr><td style='padding:6px 12px;font-weight:bold'>Reason</td><td style='padding:6px 12px'>{reason}</td></tr>" : "") +
                $"</table>"));
}
