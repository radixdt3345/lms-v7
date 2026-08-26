using System.Security.Claims;
using LMS.Infrastructure.Common;
using LMS.Infrastructure.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/departments")]
[Authorize]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departments;

    public DepartmentsController(IDepartmentService departments)
    {
        _departments = departments;
    }

    private Guid GetActorId()
    {
        var sub =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? string.Empty;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    private string GetActorEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? string.Empty;

    /// <summary>GET /api/v1/departments — all authenticated roles</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var depts = await _departments.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<DepartmentDto>>.Ok(depts));
    }

    /// <summary>GET /api/v1/departments/{id} — all authenticated roles</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dept = await _departments.GetByIdAsync(id, ct);
        if (dept is null)
            return NotFound(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Department {id} was not found.",
                    Status = 404,
                }
            );
        return Ok(ApiResponse<DepartmentDto>.Ok(dept));
    }

    /// <summary>POST /api/v1/departments — HR_ADMIN / SUPER_ADMIN only</summary>
    [HttpPost]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken ct
    )
    {
        var (dept, error) = await _departments.CreateAsync(
            request,
            GetActorId(),
            GetActorEmail(),
            ct
        );

        if (error == "DUPLICATE_DEPARTMENT_NAME")
            return Conflict(
                new ProblemDetails
                {
                    Title = "Conflict",
                    Detail =
                        "A department with the same name or code already exists.",
                    Status = 409,
                    Extensions = { ["code"] = "DUPLICATE_DEPARTMENT_NAME" },
                }
            );

        return CreatedAtAction(
            nameof(GetById),
            new { id = dept!.Id },
            ApiResponse<DepartmentDto>.Ok(dept)
        );
    }

    /// <summary>PUT /api/v1/departments/{id} — HR_ADMIN / SUPER_ADMIN only</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken ct
    )
    {
        var (dept, error) = await _departments.UpdateAsync(
            id,
            request,
            GetActorId(),
            GetActorEmail(),
            ct
        );

        if (error == "NOT_FOUND")
            return NotFound(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Department {id} was not found.",
                    Status = 404,
                }
            );

        if (error == "DUPLICATE_DEPARTMENT_NAME")
            return Conflict(
                new ProblemDetails
                {
                    Title = "Conflict",
                    Detail =
                        "A department with the same name or code already exists.",
                    Status = 409,
                    Extensions = { ["code"] = "DUPLICATE_DEPARTMENT_NAME" },
                }
            );

        return Ok(ApiResponse<DepartmentDto>.Ok(dept!));
    }

    /// <summary>DELETE /api/v1/departments/{id} — HR_ADMIN / SUPER_ADMIN only (soft deactivate)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var (success, error) = await _departments.DeactivateAsync(
            id,
            GetActorId(),
            GetActorEmail(),
            ct
        );

        if (error == "NOT_FOUND")
            return NotFound(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Department {id} was not found.",
                    Status = 404,
                }
            );

        return NoContent();
    }
}
