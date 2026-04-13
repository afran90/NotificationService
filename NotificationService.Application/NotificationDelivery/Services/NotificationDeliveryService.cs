using NotificationService.Application.NotificationDelivery.Abstractions;
using NotificationService.Application.NotificationDelivery.Contracts;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Application.NotificationDelivery.Services;

public class NotificationDeliveryService(INotificationDeliveryRepository repository) : INotificationDeliveryService
{
    public Task<NotificationDeliveryEntity?> UpdateStatusAsync(Guid deliveryId, UpdateNotificationDeliveryStatusRequest request, CancellationToken cancellationToken = default)
    {
        return repository.UpdateStatusAsync(deliveryId, request.Status, request.FailureReason, cancellationToken);
    }

    public Task<IReadOnlyList<NotificationDeliveryEntity>> GetByNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return repository.GetByNotificationAsync(notificationId, cancellationToken);
    }
}
