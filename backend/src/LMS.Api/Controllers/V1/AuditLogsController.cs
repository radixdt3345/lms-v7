using LMS.Infrastructure.AuditLogs;
using LMS.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

/// <summary>
/// F-13 Audit Trail — read-only endpoint.
/// HR_ADMIN and SUPER_ADMIN can search / filter the immutable audit log.
/// All mutating verbs are explicitly rejected (AC-65).
/// </summary>
[ApiController]
[Route("api/v1/audit-log")]
[Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogs;

    public AuditLogsController(IAuditLogService auditLogs) => _auditLogs = auditLogs;

    /// <summary>GET /api/v1/audit-log — search audit logs with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? userId,
        [FromQuery] string? actionType,
        [FromQuery] string? recordType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default
    )
    {
        var searchParams = new AuditLogSearchParams
        {
            UserId = userId,
            ActionType = actionType,
            RecordType = recordType,
            FromDate = fromDate,
            ToDate = toDate,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 200),
        };

        var (items, totalCount) = await _auditLogs.SearchAsync(searchParams, ct);

        var result = new PagedResult<AuditLogDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = searchParams.Page,
            PageSize = searchParams.PageSize,
        };

        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    // AC-65: audit log is append-only — reject all mutating verbs with 405
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    public IActionResult RejectMutations() =>
        StatusCode(StatusCodes.Status405MethodNotAllowed);
}
