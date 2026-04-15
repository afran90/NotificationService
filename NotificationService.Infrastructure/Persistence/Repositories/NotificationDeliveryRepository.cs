using Microsoft.EntityFrameworkCore;
using NotificationService.Application.NotificationDelivery.Abstractions;
using NotificationService.Domain.NotificationDelivery.Enums;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationDeliveryRepository(NotificationDbContext dbContext) : INotificationDeliveryRepository
{
    public async Task<NotificationDeliveryEntity> AddAsync(NotificationDeliveryEntity delivery, CancellationToken cancellationToken = default)
    {
        dbContext.NotificationDeliveries.Add(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);
        return delivery;
    }

    public async Task<NotificationDeliveryEntity?> UpdateStatusAsync(Guid deliveryId, DeliveryStatus status, string? reason, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.NotificationDeliveries.FirstOrDefaultAsync(x => x.Id == deliveryId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Status = status;
        entity.FailureReason = reason;
        entity.DeliveredAtUtc = status == DeliveryStatus.Sent ? DateTime.UtcNow : null;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<IReadOnlyList<NotificationDeliveryEntity>> GetByNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return await dbContext.NotificationDeliveries
            .Where(x => x.NotificationId == notificationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAttemptsAsync(Guid notificationId, string destination, CancellationToken cancellationToken = default)
    {
        return dbContext.NotificationDeliveries
            .Where(x => x.NotificationId == notificationId
                && x.Destination == destination
                && (x.Status == DeliveryStatus.Retrying || x.Status == DeliveryStatus.Failed))
            .CountAsync(cancellationToken);
    }
}
