namespace NotificationService.Domain.Notification.Enums;

public enum NotificationOutboxStatus
{
    Pending = 0,
    Processing = 1,
    Published = 2,
    Failed = 3
}
