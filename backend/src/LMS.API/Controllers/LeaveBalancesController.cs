using LMS.Api.Models.Responses;
using LMS.Infrastructure.LeaveBalances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/leave-balances")]
[Authorize]
public sealed class LeaveBalancesController(ILeaveBalanceService svc) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBalances([FromQuery] int? year)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var y = year ?? DateTime.UtcNow.Year;
        var data = await svc.GetMyBalancesAsync(userId, y);
        return Ok(new ApiResponse<IReadOnlyList<LeaveBalanceDto>> { Data = data });
    }

    [HttpGet("{employeeId:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetEmployeeBalances(Guid employeeId, [FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var data = await svc.GetEmployeeBalancesAsync(employeeId, y);
        return Ok(new ApiResponse<IReadOnlyList<LeaveBalanceDto>> { Data = data });
    }

    [HttpGet]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetAllBalances([FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var data = await svc.GetAllBalancesAsync(y);
        return Ok(new ApiResponse<IReadOnlyList<LeaveBalanceDto>> { Data = data });
    }

    [HttpPost("credit")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> CreditAnnual([FromBody] CreditAnnualRequest req)
    {
        await svc.CreditAnnualBalancesAsync(req.Year);
        return Ok(new ApiResponse<string> { Data = "Annual balances credited" });
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Adjust([FromBody] AdjustBalanceRequest req)
    {
        await svc.AdjustBalanceAsync(req);
        return Ok(new ApiResponse<string> { Data = "Balance adjusted" });
    }
}
