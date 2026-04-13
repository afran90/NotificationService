using NotificationService.Application.UserSubscription.Abstractions;
using NotificationService.Application.UserSubscription.Contracts;
using UserSubscriptionEntity = NotificationService.Domain.UserSubscription.Entities.UserSubscription;

namespace NotificationService.Application.UserSubscription.Services;

public class UserSubscriptionService(IUserSubscriptionRepository repository) : IUserSubscriptionService
{
    public async Task<UserSubscriptionEntity> CreateAsync(CreateUserSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new UserSubscriptionEntity
        {
            UserId = request.UserId,
            NotificationType = request.NotificationType,
            IsSubscribed = request.IsSubscribed
        };

        return await repository.AddAsync(entity, cancellationToken);
    }

    public Task<IReadOnlyList<UserSubscriptionEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return repository.GetByUserAsync(userId, cancellationToken);
    }
}
