namespace LMS.Infrastructure.Data.Entities;

public sealed class CompOffCredit
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly EarnedDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public decimal CreditDays { get; set; }
    public string Status { get; set; } = "Active";
    public Guid? CompOffRequestId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Employee { get; set; } = null!;
}
