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

    // GET /api/v1/reports/leave?year=2026
    [HttpGet("leave")]
    public async Task<IActionResult> GetLeaveReport([FromQuery] int year = 0, [FromQuery] Guid? employeeId = null)
    {
        if (year == 0) year = DateTime.UtcNow.Year;
        var csv = await _svc.GetLeaveReportCsvAsync(year, employeeId);
        return File(csv, "text/csv", $"leave-report-{year}.csv");
    }

    // GET /api/v1/reports/comp-off?year=2026
    [HttpGet("comp-off")]
    public async Task<IActionResult> GetCompOffReport([FromQuery] int year = 0, [FromQuery] Guid? employeeId = null)
    {
        if (year == 0) year = DateTime.UtcNow.Year;
        var csv = await _svc.GetCompOffReportCsvAsync(year, employeeId);
        return File(csv, "text/csv", $"compoff-report-{year}.csv");
    }

    // GET /api/v1/reports/leave-balances?year=2026
    [HttpGet("leave-balances")]
    public async Task<IActionResult> GetLeaveBalanceReport([FromQuery] int year = 0)
    {
        if (year == 0) year = DateTime.UtcNow.Year;
        var csv = await _svc.GetLeaveBalanceReportCsvAsync(year);
        return File(csv, "text/csv", $"leave-balances-{year}.csv");
    }
}
