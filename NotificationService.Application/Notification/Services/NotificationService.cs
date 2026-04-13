using NotificationService.Application.Abstractions.Messaging;
using NotificationService.Application.Notification.Abstractions;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Domain.Notification.Enums;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Services;

public class NotificationApplicationService(
    INotificationRepository notificationRepository,
    IMessagePublisher messagePublisher) : INotificationService
{
    public async Task<NotificationEntity> SendAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
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

    public async Task<PagedNotificationsResponse> GetByUserAsync(Guid userId, GetUserNotificationsRequest request, CancellationToken cancellationToken = default)
    {
        var take = request.PageSize + 1;
        var notifications = await notificationRepository.GetByUserAsync(userId, request.Page, take, cancellationToken);
        var hasMore = notifications.Count > request.PageSize;

        return new PagedNotificationsResponse
        {
            Items = hasMore ? notifications.Take(request.PageSize).ToList() : notifications,
            Page = request.Page,
            PageSize = request.PageSize,
            HasMore = hasMore
        };
    }

    public async Task<NotificationEntity?> MarkAsReadAsync(MarkNotificationAsReadRequest request, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.MarkAsReadAsync(request.NotificationId, cancellationToken);
        if (notification is null)
        {
            return null;
        }

        await messagePublisher.PublishAsync("notifications.read", new
        {
            notification.Id,
            notification.UserId,
            Status = NotificationStatus.Read
        }, cancellationToken);

        return notification;
    }
}
