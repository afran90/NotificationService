namespace NotificationService.Application.Notification.Contracts;

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Channel,
    string Subject,
    string Body);
