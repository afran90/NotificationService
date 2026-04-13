using System.ComponentModel.DataAnnotations;
using NotificationService.Domain.Notification.Enums;

namespace NotificationService.Application.UserSubscription.Contracts;

public sealed class CreateUserSubscriptionRequest : IValidatableObject
{
    public Guid UserId { get; init; }
    public NotificationType NotificationType { get; init; }
    public bool IsSubscribed { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserId == Guid.Empty)
        {
            yield return new ValidationResult("UserId is required.", [nameof(UserId)]);
        }
    }
}
