using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Application.Notification.Contracts;

public sealed record NotificationMessage(
    Guid NotificationId,
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    string? Metadata);
