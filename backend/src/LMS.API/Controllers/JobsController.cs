using LMS.Infrastructure.BackgroundJobs;
using LMS.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/v1/jobs")]
[Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobService _svc;
    public JobsController(IBackgroundJobService svc) => _svc = svc;

    [HttpPost("expire-comp-off")]
    public async Task<IActionResult> ExpireCompOff(CancellationToken ct)
    {
        var result = await _svc.ExpireCompOffCreditsAsync(ct);
        return Ok(new ApiResponse<string> { Data = result });
    }

    [HttpPost("reset-leave-balances")]
    public async Task<IActionResult> ResetLeaveBalances([FromQuery] int? year, CancellationToken ct)
    {
        var y = year ?? DateTime.UtcNow.Year + 1;
        var result = await _svc.ResetAnnualLeaveBalancesAsync(y, ct);
        return Ok(new ApiResponse<string> { Data = result });
    }

    [HttpPost("send-reminders")]
    public async Task<IActionResult> SendReminders(CancellationToken ct)
    {
        var result = await _svc.SendPendingLeaveRemindersAsync(ct);
        return Ok(new ApiResponse<string> { Data = result });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int count = 20, CancellationToken ct = default)
    {
        var logs = await _svc.GetRecentJobLogsAsync(count, ct);
        return Ok(new ApiResponse<IEnumerable<JobLogDto>> { Data = logs });
    }
}
