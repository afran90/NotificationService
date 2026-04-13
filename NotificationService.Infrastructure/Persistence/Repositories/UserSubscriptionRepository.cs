using Microsoft.EntityFrameworkCore;
using NotificationService.Application.UserSubscription.Abstractions;
using UserSubscriptionEntity = NotificationService.Domain.UserSubscription.Entities.UserSubscription;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class UserSubscriptionRepository(NotificationDbContext dbContext) : IUserSubscriptionRepository
{
    public async Task<UserSubscriptionEntity> AddAsync(UserSubscriptionEntity subscription, CancellationToken cancellationToken = default)
    {
        dbContext.UserSubscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<IReadOnlyList<UserSubscriptionEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSubscriptions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
