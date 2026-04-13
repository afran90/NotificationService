using NotificationService.Domain.Common;

namespace NotificationService.Domain.UserSubscription.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
