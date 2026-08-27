namespace LMS.Infrastructure.Approvals;

public interface IApprovalService
{
    Task<List<PendingApprovalDto>> GetPendingApprovalsAsync();
    Task<List<ApprovalHistoryDto>> GetApprovalHistoryAsync(string entityType, Guid entityId);
}
