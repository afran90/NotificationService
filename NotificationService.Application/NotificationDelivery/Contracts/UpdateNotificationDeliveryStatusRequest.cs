using NotificationService.Domain.NotificationDelivery.Enums;

namespace NotificationService.Application.NotificationDelivery.Contracts;

public sealed record UpdateNotificationDeliveryStatusRequest(
    DeliveryStatus Status,
    string? FailureReason);
