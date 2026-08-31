using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using LMS.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.LeaveApplications;

public sealed class LeaveApplicationService : ILeaveApplicationService
{
    private readonly LmsDbContext _db;
    private readonly IEmailService _email;

    public LeaveApplicationService(LmsDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    private static LeaveApplicationDto ToDto(LeaveApplication a) => new(
        a.Id, a.EmployeeId, a.Employee.Name,
        a.LeaveTypeId, a.LeaveType.Name,
        a.StartDate, a.EndDate, a.TotalDays,
        a.Reason, a.Status,
        a.ApprovedById, a.ApprovedBy?.Name,
        a.ApprovedAt, a.RejectionReason,
        a.CreatedAt);

    private IQueryable<LeaveApplication> WithIncludes() =>
        _db.LeaveApplications
           .Include(a => a.Employee)
           .Include(a => a.LeaveType)
           .Include(a => a.ApprovedBy);

    public async Task<IReadOnlyList<LeaveApplicationDto>> GetMyApplicationsAsync(Guid userId) =>
        await WithIncludes()
            .Where(a => a.EmployeeId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => ToDto(a))
            .ToListAsync();

    public async Task<IReadOnlyList<LeaveApplicationDto>> GetAllApplicationsAsync(string? status = null)
    {
        var q = WithIncludes().AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(a => a.Status == status);
        return await q.OrderByDescending(a => a.CreatedAt).Select(a => ToDto(a)).ToListAsync();
    }

    public async Task<LeaveApplicationDto> GetByIdAsync(Guid id)
    {
        var a = await WithIncludes().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException($"Leave application {id} not found.");
        return ToDto(a);
    }

    public async Task<LeaveApplicationDto> SubmitAsync(Guid employeeId, SubmitLeaveApplicationRequest request)
    {
        var totalDays = (decimal)(request.EndDate.DayNumber - request.StartDate.DayNumber + 1);
        var app = new LeaveApplication
        {
            EmployeeId = employeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.LeaveApplications.Add(app);
        await _db.SaveChangesAsync();

        var dto = await GetByIdAsync(app.Id);
        var employee = await _db.Users.FindAsync(employeeId);
        if (employee?.Email != null)
        {
            _ = _email.SendLeaveAppliedAsync(
                employee.Email, employee.Name,
                dto.LeaveTypeName,
                dto.StartDate.ToString("dd MMM yyyy"),
                dto.EndDate.ToString("dd MMM yyyy"));
        }

        return dto;
    }

    public async Task<LeaveApplicationDto> ApproveAsync(Guid id, Guid approverId)
    {
        var app = await _db.LeaveApplications.FindAsync(id)
            ?? throw new KeyNotFoundException($"Leave application {id} not found.");
        if (app.Status != "Pending") throw new InvalidOperationException("Only pending applications can be approved.");
        app.Status = "Approved";
        app.ApprovedById = approverId;
        app.ApprovedAt = DateTime.UtcNow;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = await GetByIdAsync(id);
        var employee = await _db.Users.FindAsync(app.EmployeeId);
        if (employee?.Email != null)
        {
            _ = _email.SendLeaveApprovedAsync(
                employee.Email, employee.Name,
                dto.LeaveTypeName,
                dto.StartDate.ToString("dd MMM yyyy"),
                dto.EndDate.ToString("dd MMM yyyy"));
        }

        return dto;
    }

    public async Task<LeaveApplicationDto> RejectAsync(Guid id, Guid rejectedById, string rejectionReason)
    {
        var app = await _db.LeaveApplications.FindAsync(id)
            ?? throw new KeyNotFoundException($"Leave application {id} not found.");
        if (app.Status != "Pending") throw new InvalidOperationException("Only pending applications can be rejected.");
        app.Status = "Rejected";
        app.ApprovedById = rejectedById;
        app.ApprovedAt = DateTime.UtcNow;
        app.RejectionReason = rejectionReason;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = await GetByIdAsync(id);
        var employee = await _db.Users.FindAsync(app.EmployeeId);
        if (employee?.Email != null)
        {
            _ = _email.SendLeaveRejectedAsync(
                employee.Email, employee.Name,
                dto.LeaveTypeName,
                dto.StartDate.ToString("dd MMM yyyy"),
                dto.EndDate.ToString("dd MMM yyyy"),
                rejectionReason);
        }

        return dto;
    }

    public async Task CancelAsync(Guid id, Guid requestingUserId)
    {
        var app = await _db.LeaveApplications.FindAsync(id)
            ?? throw new KeyNotFoundException($"Leave application {id} not found.");
        if (app.EmployeeId != requestingUserId) throw new UnauthorizedAccessException("Cannot cancel another employee's application.");
        if (app.Status == "Approved" || app.Status == "Rejected") throw new InvalidOperationException("Cannot cancel an already processed application.");
        app.Status = "Cancelled";
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = await GetByIdAsync(id);
        var employee = await _db.Users.FindAsync(requestingUserId);
        if (employee?.Email != null)
        {
            _ = _email.SendLeaveCancelledAsync(
                employee.Email, employee.Name,
                dto.LeaveTypeName,
                dto.StartDate.ToString("dd MMM yyyy"),
                dto.EndDate.ToString("dd MMM yyyy"));
        }
    }

    public async Task<LeaveApplicationDto> RevokeAsync(Guid id, Guid hrAdminId)
    {
        var app = await _db.LeaveApplications.FindAsync(id)
            ?? throw new KeyNotFoundException($"Leave application {id} not found.");
        if (app.Status != "Approved") throw new InvalidOperationException("Only approved applications can be revoked.");
        app.Status = "Revoked";
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = await GetByIdAsync(id);
        var employee = await _db.Users.FindAsync(app.EmployeeId);
        if (employee?.Email != null)
        {
            _ = _email.SendLeaveRevokedAsync(
                employee.Email, employee.Name,
                dto.LeaveTypeName,
                dto.StartDate.ToString("dd MMM yyyy"),
                dto.EndDate.ToString("dd MMM yyyy"));
        }

        return dto;
    }
}
