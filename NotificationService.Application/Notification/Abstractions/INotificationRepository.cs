using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Abstractions;

public interface INotificationRepository
{
    Task<NotificationEntity> AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
