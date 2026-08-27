using LMS.Infrastructure.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _svc;
    public DashboardController(IDashboardService svc) => _svc = svc;
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/v1/dashboard/employee
    [HttpGet("employee")]
    public async Task<IActionResult> GetEmployeeDashboard()
    {
        var data = await _svc.GetEmployeeDashboardAsync(CurrentUserId);
        return Ok(new ApiResponse<EmployeeDashboardDto> { Data = data });
    }

    // GET /api/v1/dashboard/hr
    [HttpGet("hr")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetHrDashboard()
    {
        var data = await _svc.GetHrDashboardAsync();
        return Ok(new ApiResponse<HrDashboardDto> { Data = data });
    }
}
