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
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Metadata = request.Metadata
        };

        var created = await notificationRepository.AddAsync(entity, cancellationToken);
        await messagePublisher.PublishAsync("notifications.created", new
        {
            created.Id,
            created.UserId,
            created.Type,
            created.Title,
            created.Message
        }, cancellationToken);

        return created;
    }

    public Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return notificationRepository.GetByUserAsync(userId, cancellationToken);
    }
}
