using NotificationService.Application.NotificationDelivery.Contracts;
using NotificationService.Domain.NotificationDelivery.Enums;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Application.NotificationDelivery.Services;

public interface INotificationDeliveryService
{
    Task<NotificationDeliveryEntity?> UpdateStatusAsync(Guid deliveryId, UpdateNotificationDeliveryStatusRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDeliveryEntity>> GetByNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> GetNextAttemptAsync(Guid notificationId, string destination, CancellationToken cancellationToken = default);
    Task RecordResultAsync(Guid notificationId, string destination, DeliveryStatus status, string? reason, CancellationToken cancellationToken = default);
}
