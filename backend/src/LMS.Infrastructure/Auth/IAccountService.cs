namespace LMS.Infrastructure.Auth;

public interface IAccountService
{
    Task<IReadOnlyList<LockedUserDto>> GetLockedUsersAsync(CancellationToken ct = default);
    Task<bool> UnlockUserAsync(Guid userId, CancellationToken ct = default);
}

public sealed class LockedUserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int FailedAttempts { get; init; }
    public DateTime? LockedAt { get; init; }
}
