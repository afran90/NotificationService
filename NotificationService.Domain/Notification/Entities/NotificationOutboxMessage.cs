using NotificationService.Domain.Common;
using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Domain.Notification.Entities;

public class NotificationOutboxMessage : BaseEntity
{
    public Guid NotificationId { get; set; }
    public string EventType { get; set; } = "notification.created";
    public string RoutingKey { get; set; } = "notifications.created";
    public string Payload { get; set; } = string.Empty;
    public NotificationOutboxStatus Status { get; set; } = NotificationOutboxStatus.Pending;
    public int Attempts { get; set; }
    public DateTime? LockedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public string? LastError { get; set; }
}
