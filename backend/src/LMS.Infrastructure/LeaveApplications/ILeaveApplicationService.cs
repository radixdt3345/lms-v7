namespace LMS.Infrastructure.LeaveApplications;

public interface ILeaveApplicationService
{
    Task<IReadOnlyList<LeaveApplicationDto>> GetMyApplicationsAsync(Guid userId);
    Task<IReadOnlyList<LeaveApplicationDto>> GetAllApplicationsAsync(string? status = null);
    Task<LeaveApplicationDto> GetByIdAsync(Guid id);
    Task<LeaveApplicationDto> SubmitAsync(Guid employeeId, SubmitLeaveApplicationRequest request);
    Task<LeaveApplicationDto> ApproveAsync(Guid id, Guid approverId);
    Task<LeaveApplicationDto> RejectAsync(Guid id, Guid rejectedById, string rejectionReason);
    Task CancelAsync(Guid id, Guid requestingUserId);
    Task<LeaveApplicationDto> RevokeAsync(Guid id, Guid hrAdminId);
}
