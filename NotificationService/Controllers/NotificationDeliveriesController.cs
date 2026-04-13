using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.NotificationDelivery.Contracts;
using NotificationService.Application.NotificationDelivery.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationDeliveriesController(INotificationDeliveryService notificationDeliveryService) : ControllerBase
{
    [HttpPatch("{deliveryId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid deliveryId, [FromBody] UpdateNotificationDeliveryStatusRequest request, CancellationToken cancellationToken)
    {
        var delivery = await notificationDeliveryService.UpdateStatusAsync(deliveryId, request, cancellationToken);
        if (delivery is null)
        {
            return NotFound();
        }

        return Ok(delivery);
    }

    [HttpGet("notifications/{notificationId:guid}")]
    public async Task<IActionResult> GetByNotification(Guid notificationId, CancellationToken cancellationToken)
    {
        var deliveries = await notificationDeliveryService.GetByNotificationAsync(notificationId, cancellationToken);
        return Ok(deliveries);
    }
}
