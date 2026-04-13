using NotificationService.Application.UserSubscription.Contracts;
using UserSubscriptionEntity = NotificationService.Domain.UserSubscription.Entities.UserSubscription;

namespace NotificationService.Application.UserSubscription.Services;

public interface IUserSubscriptionService
{
    Task<UserSubscriptionEntity> CreateAsync(CreateUserSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSubscriptionEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
