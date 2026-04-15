using NotificationService.Application.Abstractions.Caching;
using NotificationService.Application.Notification.Abstractions;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Application.UserSubscription.Abstractions;
using NotificationService.Domain.Notification.Enums;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Services;

public class NotificationApplicationService(
    INotificationRepository notificationRepository,
    IUserSubscriptionRepository userSubscriptionRepository,
    ICacheService cacheService,
    NotificationCacheOptions cacheOptions) : INotificationService
{
    private int CachedNotificationsLimit => cacheOptions.CachedNotificationsLimit;

    private TimeSpan CacheTtl => cacheOptions.CacheTtl;

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

        await RefreshUserNotificationCacheAsync(request.UserId, cancellationToken);

        return created;
    }

    public async Task<PagedNotificationsResponse> GetByUserAsync(Guid userId, GetUserNotificationsRequest request, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildUserNotificationsCacheKey(userId);
        var cachedNotifications = await cacheService.GetAsync<List<NotificationEntity>>(cacheKey, cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        if (cachedNotifications is not null && CanServeFromCache(cachedNotifications.Count, skip, request.PageSize))
        {
            return BuildPagedResponseFromCache(cachedNotifications, request, skip);
        }

        var response = await GetFromRepositoryAsync(userId, request, cancellationToken);

        if (cachedNotifications is null)
        {
            await RefreshUserNotificationCacheAsync(userId, cancellationToken);
        }

        return response;
    }

    public async Task<NotificationEntity?> MarkAsReadAsync(MarkNotificationAsReadRequest request, CancellationToken cancellationToken = default)
    {
        var updated = await notificationRepository.MarkAsReadAsync(request.NotificationId, cancellationToken);
        if (updated is null)
        {
            return null;
        }

        await UpdateCachedNotificationAsync(updated, cancellationToken);

        return updated;
    }

    private async Task<bool> CanSendAsync(Guid userId, NotificationType notificationType, CancellationToken cancellationToken)
    {
        var subscriptions = await userSubscriptionRepository.GetByUserAsync(userId, cancellationToken);
        var subscription = subscriptions.FirstOrDefault(x => x.NotificationType == notificationType);

        return subscription is null || subscription.IsSubscribed;
    }

    private async Task<PagedNotificationsResponse> GetFromRepositoryAsync(Guid userId, GetUserNotificationsRequest request, CancellationToken cancellationToken)
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

    private static string BuildUserNotificationsCacheKey(Guid userId)
    {
        return $"user_notifications:{userId}";
    }

    private bool CanServeFromCache(int cachedCount, int skip, int pageSize)
    {
        if (cachedCount < CachedNotificationsLimit)
        {
            return true;
        }

        return skip + pageSize <= cachedCount;
    }

    private PagedNotificationsResponse BuildPagedResponseFromCache(List<NotificationEntity> cachedNotifications, GetUserNotificationsRequest request, int skip)
    {
        var items = skip < cachedNotifications.Count
            ? cachedNotifications.Skip(skip).Take(request.PageSize).ToList()
            : [];

        var hasMore = cachedNotifications.Count switch
        {
            var cachedCount when cachedCount < CachedNotificationsLimit => skip + request.PageSize < cachedNotifications.Count,
            _ => skip + request.PageSize <= cachedNotifications.Count
        };

        return new PagedNotificationsResponse
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            HasMore = hasMore
        };
    }

    private async Task RefreshUserNotificationCacheAsync(Guid userId, CancellationToken cancellationToken)
    {
        var latestNotifications = await notificationRepository.GetByUserAsync(userId, page: 1, pageSize: CachedNotificationsLimit, cancellationToken);
        await cacheService.SetAsync(BuildUserNotificationsCacheKey(userId), latestNotifications.ToList(), CacheTtl, cancellationToken);
    }

    private async Task UpdateCachedNotificationAsync(NotificationEntity notification, CancellationToken cancellationToken)
    {
        var cacheKey = BuildUserNotificationsCacheKey(notification.UserId);
        var cachedNotifications = await cacheService.GetAsync<List<NotificationEntity>>(cacheKey, cancellationToken);
        if (cachedNotifications is null)
        {
            return;
        }

        var index = cachedNotifications.FindIndex(x => x.Id == notification.Id);
        if (index < 0)
        {
            return;
        }

        cachedNotifications[index] = notification;

        await cacheService.SetAsync(cacheKey, cachedNotifications, CacheTtl, cancellationToken);
    }
}
