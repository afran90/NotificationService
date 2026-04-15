using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Application.Notification.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var notification = await notificationService.SendAsync(request, cancellationToken);
        if (notification is null)
        {
            return BadRequest(new { message = "The user is not subscribed to this notification type." });
        }

        return Ok(notification);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] GetUserNotificationsRequest request, CancellationToken cancellationToken)
    {
        var notifications = await notificationService.GetByUserAsync(userId, request, cancellationToken);
        return Ok(notifications);
    }

    [HttpPost("read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkNotificationAsReadRequest request, CancellationToken cancellationToken)
    {
        var notification = await notificationService.MarkAsReadAsync(request, cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        return Ok(notification);
    }
}
