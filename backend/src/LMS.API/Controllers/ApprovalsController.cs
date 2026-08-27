using LMS.Infrastructure.Approvals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/v1/approvals")]
[Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _svc;
    public ApprovalsController(IApprovalService svc) => _svc = svc;

    // GET /api/v1/approvals/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var items = await _svc.GetPendingApprovalsAsync();
        return Ok(new ApiResponse<List<PendingApprovalDto>> { Data = items });
    }

    // GET /api/v1/approvals/history/{entityType}/{entityId}
    [HttpGet("history/{entityType}/{entityId:guid}")]
    public async Task<IActionResult> GetHistory(string entityType, Guid entityId)
    {
        var items = await _svc.GetApprovalHistoryAsync(entityType, entityId);
        return Ok(new ApiResponse<List<ApprovalHistoryDto>> { Data = items });
    }
}
