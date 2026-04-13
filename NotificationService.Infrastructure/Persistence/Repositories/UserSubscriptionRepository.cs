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

    public async Task<UserSubscriptionEntity> UpsertAsync(UserSubscriptionEntity subscription, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == subscription.UserId && x.NotificationType == subscription.NotificationType, cancellationToken);

        if (existing is null)
        {
            dbContext.UserSubscriptions.Add(subscription);
            await dbContext.SaveChangesAsync(cancellationToken);
            return subscription;
        }

        existing.IsSubscribed = subscription.IsSubscribed;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<IReadOnlyList<UserSubscriptionEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSubscriptions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
