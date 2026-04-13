using NotificationService.Domain.Common;
using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Domain.UserSubscription.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType NotificationType { get; set; }
    public bool IsSubscribed { get; set; } = true;
}
