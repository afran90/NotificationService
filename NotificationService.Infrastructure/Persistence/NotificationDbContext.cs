using Microsoft.EntityFrameworkCore;
using NotificationEntity = NotificationService.Domain.Notification.Entities.Notification;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;
using UserSubscriptionEntity = NotificationService.Domain.UserSubscription.Entities.UserSubscription;
using NotificationTemplateEntity = NotificationService.Domain.Notification.Entities.NotificationTemplate;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();
    public DbSet<UserSubscriptionEntity> UserSubscriptions => Set<UserSubscriptionEntity>();
    public DbSet<NotificationDeliveryEntity> NotificationDeliveries => Set<NotificationDeliveryEntity>();
    public DbSet<NotificationTemplateEntity> NotificationTemplates => Set<NotificationTemplateEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Message).IsRequired();
            entity.Property(x => x.Metadata).HasColumnType("jsonb");
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.DeliveredAtUtc);
            entity.Property(x => x.UpdatedAtUtc);

            // Indexes
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.HasIndex(x => x.CreatedAtUtc);

            // Relationships
            entity.HasMany(x => x.Deliveries)
                .WithOne(x => x.Notification)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSubscriptionEntity>(entity =>
        {
            entity.ToTable("user_subscriptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.NotificationType).IsRequired();
            entity.Property(x => x.IsSubscribed).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc);

            // Indexes
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.UserId, x.NotificationType }).IsUnique();
        });

        modelBuilder.Entity<NotificationDeliveryEntity>(entity =>
        {
            entity.ToTable("notification_deliveries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.NotificationId).IsRequired();
            entity.Property(x => x.Destination).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(1024);
            entity.Property(x => x.DeliveredAtUtc);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc);

            // Indexes
            entity.HasIndex(x => x.NotificationId);
            entity.HasIndex(x => x.Status);

            // Relationships
            entity.HasOne(x => x.Notification)
                .WithMany(x => x.Deliveries)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationTemplateEntity>(entity =>
        {
            entity.ToTable("notification_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TitleTemplate).IsRequired();
            entity.Property(x => x.BodyTemplate).IsRequired();
            entity.Property(x => x.ChannelType).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc);

            // Indexes
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.ChannelType);
        });
    }
}
