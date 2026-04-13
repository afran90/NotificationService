using NotificationService.Application.Notification.Contracts;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Services;

public interface INotificationService
{
    Task<NotificationEntity> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
