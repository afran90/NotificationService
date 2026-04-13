namespace NotificationService.Application.UserSubscription.Contracts;

public sealed record CreateUserSubscriptionRequest(
    Guid UserId,
    string Channel,
    string Endpoint);
