using System.ComponentModel.DataAnnotations;
using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Application.Notification.Contracts;

public sealed class CreateNotificationRequest : IValidatableObject
{
    public Guid UserId { get; init; }
    public NotificationType Type { get; init; }

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Message { get; init; } = string.Empty;

    public string? Metadata { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserId == Guid.Empty)
        {
            yield return new ValidationResult("UserId is required.", [nameof(UserId)]);
        }
    }
}

public sealed class GetUserNotificationsRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public sealed class MarkNotificationAsReadRequest : IValidatableObject
{
    public Guid NotificationId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NotificationId == Guid.Empty)
        {
            yield return new ValidationResult("NotificationId is required.", [nameof(NotificationId)]);
        }
    }
}
