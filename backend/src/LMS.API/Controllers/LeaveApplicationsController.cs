using LMS.API.Models;
using LMS.Infrastructure.LeaveApplications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/v1/leave-applications")]
[Authorize]
public sealed class LeaveApplicationsController : ControllerBase
{
    private readonly ILeaveApplicationService _svc;
    public LeaveApplicationsController(ILeaveApplicationService svc) => _svc = svc;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());

    [HttpGet("me")]
    public async Task<IActionResult> GetMyApplications()
    {
        var result = await _svc.GetMyApplicationsAsync(CurrentUserId);
        return Ok(new ApiResponse<IReadOnlyList<LeaveApplicationDto>> { Data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _svc.GetByIdAsync(id);
        return Ok(new ApiResponse<LeaveApplicationDto> { Data = result });
    }

    [HttpGet]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN,MANAGER")]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var result = await _svc.GetAllApplicationsAsync(status);
        return Ok(new ApiResponse<IReadOnlyList<LeaveApplicationDto>> { Data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitLeaveApplicationRequest request)
    {
        var result = await _svc.SubmitAsync(CurrentUserId, request);
        return Ok(new ApiResponse<LeaveApplicationDto> { Data = result });
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN,MANAGER")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _svc.ApproveAsync(id, CurrentUserId);
        return Ok(new ApiResponse<LeaveApplicationDto> { Data = result });
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN,MANAGER")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveApplicationRequest request)
    {
        var result = await _svc.RejectAsync(id, CurrentUserId, request.RejectionReason);
        return Ok(new ApiResponse<LeaveApplicationDto> { Data = result });
    }

    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _svc.CancelAsync(id, CurrentUserId);
        return Ok(new ApiResponse<object?> { Data = null });
    }

    [HttpPut("{id:guid}/revoke")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var result = await _svc.RevokeAsync(id, CurrentUserId);
        return Ok(new ApiResponse<LeaveApplicationDto> { Data = result });
    }
}
