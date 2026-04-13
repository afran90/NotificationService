using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Application.Notification.Contracts;

public sealed record CreateNotificationRequest(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Metadata = null);
