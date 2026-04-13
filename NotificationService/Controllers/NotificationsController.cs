using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Application.Notification.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var notification = await notificationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByUser), new { userId = notification.UserId }, notification);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await notificationService.GetByUserAsync(userId, cancellationToken);
        return Ok(notifications);
    }
}
