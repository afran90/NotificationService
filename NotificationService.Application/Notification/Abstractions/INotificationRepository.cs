using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Abstractions;

public interface INotificationRepository
{
    Task<NotificationEntity> AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<NotificationEntity?> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
