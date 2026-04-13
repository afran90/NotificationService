using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Application.Notification.Contracts;

public sealed class PagedNotificationsResponse
{
    public required IReadOnlyList<NotificationEntity> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore { get; init; }
}
