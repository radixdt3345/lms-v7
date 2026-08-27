namespace LMS.Infrastructure.CompOff;

public interface ICompOffService
{
    Task<IReadOnlyList<CompOffRequestDto>> GetMyRequestsAsync(Guid userId);
    Task<IReadOnlyList<CompOffRequestDto>> GetAllRequestsAsync(string? status = null);
    Task<CompOffRequestDto> SubmitRequestAsync(Guid employeeId, SubmitCompOffRequest request);
    Task<CompOffRequestDto> ApproveRequestAsync(Guid id, Guid approverId);
    Task<CompOffRequestDto> RejectRequestAsync(Guid id, Guid rejectedById, string reason);
    Task<IReadOnlyList<CompOffCreditDto>> GetMyCreditsAsync(Guid userId);
}
