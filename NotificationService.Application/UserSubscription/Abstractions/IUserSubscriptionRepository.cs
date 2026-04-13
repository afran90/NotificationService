using UserSubscriptionEntity = NotificationService.Domain.UserSubscription.Entities.UserSubscription;

namespace NotificationService.Application.UserSubscription.Abstractions;

public interface IUserSubscriptionRepository
{
    Task<UserSubscriptionEntity> AddAsync(UserSubscriptionEntity subscription, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSubscriptionEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
