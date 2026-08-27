using LMS.API.Models;
using LMS.Infrastructure.CompOff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController, Route("api/v1/comp-off"), Authorize]
public sealed class CompOffController : ControllerBase
{
    private readonly ICompOffService _svc;
    public CompOffController(ICompOffService svc) => _svc = svc;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    [HttpGet("requests/me")]
    public async Task<IActionResult> GetMyRequests() =>
        Ok(new ApiResponse<IReadOnlyList<CompOffRequestDto>> { Data = await _svc.GetMyRequestsAsync(CurrentUserId) });

    [HttpGet("requests"), Authorize(Roles = "HR_ADMIN,SUPER_ADMIN,MANAGER")]
    public async Task<IActionResult> GetAllRequests([FromQuery] string? status = null) =>
        Ok(new ApiResponse<IReadOnlyList<CompOffRequestDto>> { Data = await _svc.GetAllRequestsAsync(status) });

    [HttpPost("requests")]
    public async Task<IActionResult> Submit([FromBody] SubmitCompOffRequest request) =>
        Ok(new ApiResponse<CompOffRequestDto> { Data = await _svc.SubmitRequestAsync(CurrentUserId, request) });

    [HttpPut("requests/{id:guid}/approve"), Authorize(Roles = "HR_ADMIN,SUPER_ADMIN,MANAGER")]
    public async Task<IActionResult> Approve(Guid id) =>
        Ok(new ApiResponse<CompOffRequestDto> { Data = await _svc.ApproveRequestAsync(id, CurrentUserId) });

    [HttpPut("requests/{id:guid}/reject"), Authorize(Roles = "HR_ADMIN,SUPER_ADMIN,MANAGER")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectCompOffRequest request) =>
        Ok(new ApiResponse<CompOffRequestDto> { Data = await _svc.RejectRequestAsync(id, CurrentUserId, request.RejectionReason) });

    [HttpGet("credits/me")]
    public async Task<IActionResult> GetMyCredits() =>
        Ok(new ApiResponse<IReadOnlyList<CompOffCreditDto>> { Data = await _svc.GetMyCreditsAsync(CurrentUserId) });
}
