using LMS.Infrastructure.Common;
using LMS.Infrastructure.PublicHolidays;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/holidays")]
[Authorize]
public sealed class PublicHolidaysController(IPublicHolidayService svc) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] int year = 0, CancellationToken ct = default)
    {
        if (year == 0) year = DateTime.UtcNow.Year;
        var items = await svc.ListAsync(year, ct);
        return Ok(ApiResponse<List<PublicHolidayDto>>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await svc.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(ApiResponse<PublicHolidayDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreatePublicHolidayRequest req, CancellationToken ct)
    {
        var dto = await svc.CreateAsync(req, ct);
        return StatusCode(201, ApiResponse<PublicHolidayDto>.Ok(dto));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePublicHolidayRequest req, CancellationToken ct)
    {
        var dto = await svc.UpdateAsync(id, req, ct);
        return dto is null ? NotFound() : Ok(ApiResponse<PublicHolidayDto>.Ok(dto));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await svc.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("bulk-import")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> BulkImport([FromBody] BulkImportRequest req, CancellationToken ct)
    {
        var preview = await svc.BulkImportAsync(req, ct);
        return Ok(ApiResponse<BulkImportPreview>.Ok(preview));
    }
}
