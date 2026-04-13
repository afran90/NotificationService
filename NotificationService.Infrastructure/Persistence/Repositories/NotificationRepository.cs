using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Notification.Abstractions;
using NotificationService.Domain.Notification.Enums;
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

    public async Task<IReadOnlyList<NotificationEntity>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        return await dbContext.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationEntity?> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (entity.Status != NotificationStatus.Read)
        {
            entity.Status = NotificationStatus.Read;
            entity.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }
}
