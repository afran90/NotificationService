using Microsoft.EntityFrameworkCore;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;
using UserSubscriptionEntity = NotificationService.Domain.UserSubscription.Entities.UserSubscription;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();
    public DbSet<UserSubscriptionEntity> UserSubscriptions => Set<UserSubscriptionEntity>();
    public DbSet<NotificationDeliveryEntity> NotificationDeliveries => Set<NotificationDeliveryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Channel).HasMaxLength(64);
            entity.Property(x => x.Subject).HasMaxLength(256);
            entity.HasMany(x => x.Deliveries)
                .WithOne(x => x.Notification)
                .HasForeignKey(x => x.NotificationId);
        });

        modelBuilder.Entity<UserSubscriptionEntity>(entity =>
        {
            entity.ToTable("user_subscriptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Channel).HasMaxLength(64);
            entity.Property(x => x.Endpoint).HasMaxLength(512);
        });

        modelBuilder.Entity<NotificationDeliveryEntity>(entity =>
        {
            entity.ToTable("notification_deliveries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Destination).HasMaxLength(512);
            entity.Property(x => x.FailureReason).HasMaxLength(1024);
        });
    }
}
