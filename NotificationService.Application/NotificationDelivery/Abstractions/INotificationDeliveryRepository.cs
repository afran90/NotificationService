using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;
using NotificationService.Domain.NotificationDelivery.Enums;

namespace NotificationService.Application.NotificationDelivery.Abstractions;

public interface INotificationDeliveryRepository
{
    Task<NotificationDeliveryEntity> AddAsync(NotificationDeliveryEntity delivery, CancellationToken cancellationToken = default);
    Task<NotificationDeliveryEntity?> UpdateStatusAsync(Guid deliveryId, DeliveryStatus status, string? reason, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDeliveryEntity>> GetByNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
