using LMS.Infrastructure.Common;
using LMS.Infrastructure.LeaveTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/leave-types")]
[Authorize]
public sealed class LeaveTypesController : ControllerBase
{
    private readonly ILeaveTypeService _leaveTypes;

    public LeaveTypesController(ILeaveTypeService leaveTypes)
    {
        _leaveTypes = leaveTypes;
    }

    /// <summary>GET /api/v1/leave-types — all authenticated roles</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var types = await _leaveTypes.ListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<LeaveTypeDto>>.Ok(types));
    }

    /// <summary>GET /api/v1/leave-types/{id} — all authenticated roles</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var lt = await _leaveTypes.GetByIdAsync(id, ct);
        if (lt is null)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Leave type {id} was not found.",
                Status = 404,
            });
        return Ok(ApiResponse<LeaveTypeDto>.Ok(lt));
    }

    /// <summary>POST /api/v1/leave-types — HR_ADMIN / SUPER_ADMIN only</summary>
    [HttpPost]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeaveTypeRequest request,
        CancellationToken ct)
    {
        var (lt, error) = await _leaveTypes.CreateAsync(request, ct);

        if (error == "DUPLICATE_LEAVE_TYPE")
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = "A leave type with the same name or code already exists.",
                Status = 409,
                Extensions = { ["code"] = "DUPLICATE_LEAVE_TYPE" },
            });

        return CreatedAtAction(
            nameof(GetById),
            new { id = lt!.Id },
            ApiResponse<LeaveTypeDto>.Ok(lt));
    }

    /// <summary>PUT /api/v1/leave-types/{id} — HR_ADMIN / SUPER_ADMIN only</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateLeaveTypeRequest request,
        CancellationToken ct)
    {
        var (lt, error) = await _leaveTypes.UpdateAsync(id, request, ct);

        if (error == "NOT_FOUND")
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Leave type {id} was not found.",
                Status = 404,
            });

        if (error == "DUPLICATE_LEAVE_TYPE")
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = "A leave type with the same name or code already exists.",
                Status = 409,
                Extensions = { ["code"] = "DUPLICATE_LEAVE_TYPE" },
            });

        return Ok(ApiResponse<LeaveTypeDto>.Ok(lt!));
    }

    /// <summary>DELETE /api/v1/leave-types/{id} — HR_ADMIN / SUPER_ADMIN only (deactivate)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var found = await _leaveTypes.DeactivateAsync(id, ct);
        if (!found)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Leave type {id} was not found.",
                Status = 404,
            });
        return NoContent();
    }
}
