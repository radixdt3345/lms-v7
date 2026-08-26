using System.Security.Claims;
using LMS.Infrastructure.Common;
using LMS.Infrastructure.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employees;

    public EmployeesController(IEmployeeService employees)
    {
        _employees = employees;
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

    /// <summary>GET /api/v1/employees — HR_ADMIN / SUPER_ADMIN</summary>
    [HttpGet]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var list = await _employees.ListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<EmployeeDto>>.Ok(list));
    }

    /// <summary>GET /api/v1/employees/me — any authenticated user</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var actorId = GetActorId();
        var employee = await _employees.GetMeAsync(actorId, ct);
        if (employee is null)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = "Your profile was not found.",
                Status = 404,
            });
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    /// <summary>PUT /api/v1/employees/me — any authenticated user (name + phone only)</summary>
    [HttpPut("me")]
    public async Task<IActionResult> SelfEdit([FromBody] SelfEditRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        var (employee, error) = await _employees.SelfEditAsync(actorId, req, ct);

        if (error == "NOT_FOUND")
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = "Your profile was not found.",
                Status = 404,
            });

        return Ok(ApiResponse<EmployeeDto>.Ok(employee!));
    }

    /// <summary>GET /api/v1/employees/team — MANAGER with direct reports</summary>
    [HttpGet("team")]
    [Authorize(Roles = "MANAGER,HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetTeam(CancellationToken ct)
    {
        var actorId = GetActorId();
        var team = await _employees.GetTeamAsync(actorId, ct);
        return Ok(ApiResponse<IReadOnlyList<EmployeeDto>>.Ok(team));
    }

    /// <summary>GET /api/v1/employees/{id} — HR_ADMIN / SUPER_ADMIN</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var employee = await _employees.GetByIdAsync(id, ct);
        if (employee is null)
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Employee {id} was not found.",
                Status = 404,
            });
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    /// <summary>POST /api/v1/employees — HR_ADMIN / SUPER_ADMIN (AC-16)</summary>
    [HttpPost]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest req,
        CancellationToken ct)
    {
        var (employee, error) = await _employees.CreateAsync(
            req, GetActorId(), GetActorEmail(), ct);

        if (error == "DUPLICATE_EMAIL")
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = "An employee with this email already exists.",
                Status = 409,
                Extensions = { ["code"] = "DUPLICATE_EMAIL" },
            });

        return CreatedAtAction(
            nameof(GetById),
            new { id = employee!.Id },
            ApiResponse<EmployeeDto>.Ok(employee));
    }

    /// <summary>PUT /api/v1/employees/{id} — HR_ADMIN / SUPER_ADMIN</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest req,
        CancellationToken ct)
    {
        var (employee, error) = await _employees.UpdateAsync(
            id, req, GetActorId(), GetActorEmail(), ct);

        if (error == "NOT_FOUND")
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Employee {id} was not found.",
                Status = 404,
            });

        if (error == "MANAGER_HAS_ACTIVE_REPORTS")
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = "Cannot demote a manager who still has active direct reports.",
                Status = 409,
                Extensions = { ["code"] = "MANAGER_HAS_ACTIVE_REPORTS" },
            });

        return Ok(ApiResponse<EmployeeDto>.Ok(employee!));
    }

    /// <summary>DELETE /api/v1/employees/{id} — HR_ADMIN / SUPER_ADMIN (soft deactivate, AC-19)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var (success, error) = await _employees.DeactivateAsync(
            id, GetActorId(), GetActorEmail(), ct);

        if (error == "NOT_FOUND")
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Employee {id} was not found.",
                Status = 404,
            });

        return NoContent();
    }

    /// <summary>POST /api/v1/employees/{id}/anonymise — SUPER_ADMIN only (GDPR Art.17)</summary>
    [HttpPost("{id:guid}/anonymise")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Anonymise(Guid id, CancellationToken ct)
    {
        var (success, error) = await _employees.AnonymiseAsync(
            id, GetActorId(), GetActorEmail(), ct);

        if (error == "NOT_FOUND")
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Employee {id} was not found.",
                Status = 404,
            });

        return NoContent();
    }
}
