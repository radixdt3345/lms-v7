using LMS.Infrastructure.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _svc;
    public NotificationsController(INotificationService svc) => _svc = svc;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/v1/notifications
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] bool unreadOnly = false)
    {
        var items = await _svc.GetMyNotificationsAsync(CurrentUserId, unreadOnly);
        return Ok(new ApiResponse<List<NotificationDto>> { Data = items });
    }

    // GET /api/v1/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _svc.GetUnreadCountAsync(CurrentUserId);
        return Ok(new ApiResponse<int> { Data = count });
    }

    // PUT /api/v1/notifications/mark-read
    [HttpPut("mark-read")]
    public async Task<IActionResult> MarkRead([FromBody] MarkReadRequest req)
    {
        await _svc.MarkAsReadAsync(CurrentUserId, req.NotificationIds);
        return Ok(new ApiResponse<bool> { Data = true });
    }
}
