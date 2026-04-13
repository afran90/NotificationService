using NotificationService.Application.Notification.Contracts;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Services;

public interface INotificationService
{
    Task<NotificationEntity> SendAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<PagedNotificationsResponse> GetByUserAsync(Guid userId, GetUserNotificationsRequest request, CancellationToken cancellationToken = default);
    Task<NotificationEntity?> MarkAsReadAsync(MarkNotificationAsReadRequest request, CancellationToken cancellationToken = default);
}
