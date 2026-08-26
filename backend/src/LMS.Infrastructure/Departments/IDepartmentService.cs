namespace LMS.Infrastructure.Departments;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Code,
    int OverlapLimit,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateDepartmentRequest(
    string Name,
    string Code,
    int OverlapLimit
);

public record UpdateDepartmentRequest(
    string? Name,
    string? Code,
    int? OverlapLimit,
    string? Status
);

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken ct = default);
    Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(DepartmentDto? Dept, string? Error)> CreateAsync(
        CreateDepartmentRequest request,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default
    );
    Task<(DepartmentDto? Dept, string? Error)> UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default
    );
    Task<(bool Success, string? Error)> DeactivateAsync(
        Guid id,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default
    );
}
