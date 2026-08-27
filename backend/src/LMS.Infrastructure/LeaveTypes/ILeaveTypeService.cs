namespace LMS.Infrastructure.LeaveTypes;

public interface ILeaveTypeService
{
    Task<IReadOnlyList<LeaveTypeDto>> ListAsync(CancellationToken ct = default);
    Task<LeaveTypeDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(LeaveTypeDto? Dto, string? Error)> CreateAsync(CreateLeaveTypeRequest req, CancellationToken ct = default);
    Task<(LeaveTypeDto? Dto, string? Error)> UpdateAsync(Guid id, UpdateLeaveTypeRequest req, CancellationToken ct = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default);
}
