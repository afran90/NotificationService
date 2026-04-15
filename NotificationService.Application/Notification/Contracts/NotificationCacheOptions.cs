namespace NotificationService.Application.Notification.Contracts;

public sealed class NotificationCacheOptions
{
    public const string SectionName = "NotificationCache";

    public int CachedNotificationsLimit { get; init; }
    public TimeSpan CacheTtl { get; init; }
}
