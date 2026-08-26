namespace LMS.Infrastructure.Employees;

public sealed record EmployeeDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string Role,
    string Status,
    string? JobTitle,
    DateOnly? DateOfJoining,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? ReportingManagerId,
    string? ReportingManagerName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateEmployeeRequest(
    string Name,
    string Email,
    string? Phone,
    string? JobTitle,
    DateOnly? DateOfJoining,
    Guid? DepartmentId,
    Guid? ReportingManagerId
);

public sealed record UpdateEmployeeRequest(
    string? Name,
    string? Phone,
    string? JobTitle,
    DateOnly? DateOfJoining,
    Guid? DepartmentId,
    bool ClearDepartment,
    Guid? ReportingManagerId,
    bool ClearReportingManager,
    string? Role,
    string? Status
);

public sealed record SelfEditRequest(
    string Name,
    string? Phone
);
