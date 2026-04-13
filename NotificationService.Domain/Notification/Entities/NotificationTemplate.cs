using NotificationService.Domain.Common;
using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Domain.Notification.Entities;

public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public NotificationType ChannelType { get; set; }
}
