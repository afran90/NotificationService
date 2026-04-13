using NotificationService.Application.Abstractions.Messaging;
using NotificationService.Application.Notification.Abstractions;
using NotificationService.Application.Notification.Contracts;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Services;

public class NotificationApplicationService(
    INotificationRepository notificationRepository,
    IMessagePublisher messagePublisher) : INotificationService
{
    public async Task<NotificationEntity> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new NotificationEntity
        {
            UserId = request.UserId,
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body
        };

        var created = await notificationRepository.AddAsync(entity, cancellationToken);
        await messagePublisher.PublishAsync("notifications.created", new
        {
            created.Id,
            created.UserId,
            created.Channel,
            created.Subject,
            created.Body
        }, cancellationToken);

        return created;
    }

    public Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return notificationRepository.GetByUserAsync(userId, cancellationToken);
    }
}
