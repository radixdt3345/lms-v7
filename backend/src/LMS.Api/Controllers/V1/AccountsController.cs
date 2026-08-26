using LMS.Infrastructure.Auth;
using LMS.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api.Controllers.V1;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountService _accounts;

    public AccountsController(IAccountService accounts)
    {
        _accounts = accounts;
    }

    [HttpGet("locked")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> GetLockedAccounts(CancellationToken ct = default)
    {
        var users = await _accounts.GetLockedUsersAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<LockedUserDto>>.Ok(users));
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Roles = "HR_ADMIN,SUPER_ADMIN")]
    public async Task<IActionResult> UnlockAccount(Guid id, CancellationToken ct = default)
    {
        var unlocked = await _accounts.UnlockUserAsync(id, ct);
        if (!unlocked)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "User not found or not locked",
                    Detail = $"No locked user with id '{id}' was found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        return Ok(ApiResponse<bool>.Ok(true));
    }
}
