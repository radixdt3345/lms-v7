namespace LMS.Infrastructure.AuditLogs;

/// <summary>Filter / pagination bag passed to IAuditLogService.SearchAsync.</summary>
public sealed class AuditLogSearchParams
{
    public Guid? UserId { get; set; }
    public string? ActionType { get; set; }
    public string? RecordType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}