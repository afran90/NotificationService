using NotificationService.Application.Notification.Abstractions;
using NotificationService.Application.NotificationDelivery.Abstractions;
using NotificationService.Application.NotificationDelivery.Contracts;
using NotificationService.Domain.NotificationDelivery.Enums;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Application.NotificationDelivery.Services;

public class NotificationDeliveryService(
    INotificationDeliveryRepository deliveryRepository,
    INotificationRepository notificationRepository) : INotificationDeliveryService
{
    public Task<NotificationDeliveryEntity?> UpdateStatusAsync(Guid deliveryId, UpdateNotificationDeliveryStatusRequest request, CancellationToken cancellationToken = default)
    {
        return deliveryRepository.UpdateStatusAsync(deliveryId, request.Status, request.FailureReason, cancellationToken);
    }

    public Task<IReadOnlyList<NotificationDeliveryEntity>> GetByNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return deliveryRepository.GetByNotificationAsync(notificationId, cancellationToken);
    }

    public async Task<int> GetNextAttemptAsync(Guid notificationId, string destination, CancellationToken cancellationToken = default)
    {
        var attempts = await deliveryRepository.CountAttemptsAsync(notificationId, destination, cancellationToken);
        return attempts + 1;
    }

    public async Task RecordResultAsync(
        Guid notificationId,
        string destination,
        DeliveryStatus status,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var delivery = new NotificationDeliveryEntity
        {
            NotificationId = notificationId,
            Destination = destination,
            Status = status,
            FailureReason = reason,
            DeliveredAtUtc = status == DeliveryStatus.Sent ? now : null
        };

        await deliveryRepository.AddAsync(delivery, cancellationToken);

        if (status == DeliveryStatus.Sent)
        {
            await notificationRepository.MarkAsDeliveredAsync(notificationId, now, cancellationToken);
        }
    }
}
