using LMS.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    private static readonly DateTime StartTime = DateTime.UtcNow;

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new ApiResponse<object>
        {
            Data = new
            {
                Version = "1.0.0",
                Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Uptime = (DateTime.UtcNow - StartTime).ToString(@"hh\:mm\:ss"),
                Timestamp = DateTime.UtcNow
            }
        });
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new ApiResponse<object>
        {
            Data = new { Status = "Healthy", Timestamp = DateTime.UtcNow }
        });
    }
}
