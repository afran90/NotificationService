using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Notification.Services;
using NotificationService.Application.UserSubscription.Services;
using NotificationService.Application.NotificationDelivery.Services;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationApplicationService>();
        services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
        services.AddScoped<INotificationDeliveryService, NotificationDeliveryService>();

        return services;
    }
}
