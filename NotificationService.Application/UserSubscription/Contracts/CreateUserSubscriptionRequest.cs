using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Application.UserSubscription.Contracts;

public sealed record CreateUserSubscriptionRequest(
    Guid UserId,
    NotificationType NotificationType,
    bool IsSubscribed = true);
