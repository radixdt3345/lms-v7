namespace LMS.Infrastructure.Data.Entities;

public sealed class ScheduledJobLog
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Running | Success | Failed
    public string? Details { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? TriggeredBy { get; set; }
}
