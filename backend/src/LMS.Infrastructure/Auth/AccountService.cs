using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Auth;

public sealed class AccountService : IAccountService
{
    private readonly LmsDbContext _db;

    public AccountService(LmsDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LockedUserDto>> GetLockedUsersAsync(
        CancellationToken ct = default
    )
    {
        return await _db
            .Users.Where(u => u.Status == "Locked" && u.DeletedAt == null)
            .OrderBy(u => u.LockedAt)
            .Select(u => new LockedUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                FailedAttempts = u.FailedAttempts,
                LockedAt = u.LockedAt,
            })
            .ToListAsync(ct);
    }

    public async Task<bool> UnlockUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db
            .Users.Where(u => u.Id == userId && u.Status == "Locked" && u.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            return false;
        }

        user.Status = "Active";
        user.FailedAttempts = 0;
        user.LockedAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
