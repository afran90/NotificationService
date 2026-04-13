namespace NotificationService.Domain.NotificationDelivery.Enums;

public enum DeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Retrying = 3
}
