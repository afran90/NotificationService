using NotificationService.Domain.Common;
using NotificationService.Domain.NotificationDelivery.Enums;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Domain.NotificationDelivery.Entities;

public class NotificationDelivery : BaseEntity
{
    public Guid NotificationId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public NotificationEntity Notification { get; set; } = null!;
}
