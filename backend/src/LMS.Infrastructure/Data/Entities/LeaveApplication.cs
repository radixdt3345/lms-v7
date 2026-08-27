namespace LMS.Infrastructure.Data.Entities;

public sealed class LeaveApplication
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected | Cancelled
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
    public User? ApprovedBy { get; set; }
}
