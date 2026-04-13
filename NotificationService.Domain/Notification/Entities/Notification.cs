using NotificationService.Domain.Common;
using NotificationService.Domain.Notification.Enums;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Domain.Notification.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
    public DateTime? DeliveredAtUtc { get; set; }
    public ICollection<NotificationDeliveryEntity> Deliveries { get; set; } = new List<NotificationDeliveryEntity>();
}
