namespace LMS.Infrastructure.Employees;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> ListAsync(CancellationToken ct = default);

    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(EmployeeDto? Employee, string? Error)> CreateAsync(
        CreateEmployeeRequest req,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default);

    Task<(EmployeeDto? Employee, string? Error)> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest req,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default);

    Task<(bool Success, string? Error)> DeactivateAsync(
        Guid id,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default);

    Task<EmployeeDto?> GetMeAsync(Guid userId, CancellationToken ct = default);

    Task<(EmployeeDto? Employee, string? Error)> SelfEditAsync(
        Guid userId,
        SelfEditRequest req,
        CancellationToken ct = default);

    Task<IReadOnlyList<EmployeeDto>> GetTeamAsync(Guid managerId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> AnonymiseAsync(
        Guid id,
        Guid actorId,
        string actorEmail,
        CancellationToken ct = default);
}
