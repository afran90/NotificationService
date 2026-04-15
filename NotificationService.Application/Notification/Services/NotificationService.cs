using NotificationService.Application.Notification.Abstractions;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Application.UserSubscription.Abstractions;
using NotificationService.Domain.Notification.Enums;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Services;

public class NotificationApplicationService(
    INotificationRepository notificationRepository,
    IUserSubscriptionRepository userSubscriptionRepository) : INotificationService
{
    public async Task<NotificationEntity?> SendAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (!await CanSendAsync(request.UserId, request.Type, cancellationToken))
        {
            return null;
        }

        var entity = new NotificationEntity
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Metadata = request.Metadata
        };

        var created = await notificationRepository.AddAsync(entity, new NotificationMessage(
            entity.Id,
            entity.UserId,
            entity.Type,
            entity.Title,
            entity.Message,
            entity.Metadata), cancellationToken);

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

    public Task<NotificationEntity?> MarkAsReadAsync(MarkNotificationAsReadRequest request, CancellationToken cancellationToken = default)
    {
        return notificationRepository.MarkAsReadAsync(request.NotificationId, cancellationToken);
    }

    private async Task<bool> CanSendAsync(Guid userId, NotificationType notificationType, CancellationToken cancellationToken)
    {
        var subscriptions = await userSubscriptionRepository.GetByUserAsync(userId, cancellationToken);
        var subscription = subscriptions.FirstOrDefault(x => x.NotificationType == notificationType);

        return subscription is null || subscription.IsSubscribed;
    }
}
