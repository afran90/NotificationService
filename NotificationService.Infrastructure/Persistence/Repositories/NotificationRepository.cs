using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Notification.Abstractions;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public async Task<NotificationEntity> AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
