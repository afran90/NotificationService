using NotificationService.Domain.Common;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Domain.Notification.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ICollection<NotificationDeliveryEntity> Deliveries { get; set; } = new List<NotificationDeliveryEntity>();
}
