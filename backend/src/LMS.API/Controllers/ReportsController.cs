using LMS.Infrastructure.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _svc;
    public ReportsController(IReportService svc) => _svc = svc;

    [HttpGet("leave")]
    public async Task<IActionResult> LeaveReport([FromQuery] int? year, CancellationToken ct)
    {
        var csv = await _svc.GenerateLeaveReportAsync(year, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"leave-report-{year ?? DateTime.UtcNow.Year}.csv");
    }

    [HttpGet("comp-off")]
    public async Task<IActionResult> CompOffReport([FromQuery] int? year, CancellationToken ct)
    {
        var csv = await _svc.GenerateCompOffReportAsync(year, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"compoff-report-{year ?? DateTime.UtcNow.Year}.csv");
    }

    [HttpGet("leave-balances")]
    public async Task<IActionResult> LeaveBalanceReport([FromQuery] int? year, CancellationToken ct)
    {
        var csv = await _svc.GenerateLeaveBalanceReportAsync(year, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"leave-balance-report-{year ?? DateTime.UtcNow.Year}.csv");
    }
}
