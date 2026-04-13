using NotificationService.Application.NotificationDelivery.Contracts;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Application.NotificationDelivery.Services;

public interface INotificationDeliveryService
{
    Task<NotificationDeliveryEntity?> UpdateStatusAsync(Guid deliveryId, UpdateNotificationDeliveryStatusRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDeliveryEntity>> GetByNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
