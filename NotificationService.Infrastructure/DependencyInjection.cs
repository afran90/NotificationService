using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Abstractions.Caching;
using NotificationService.Application.Abstractions.Messaging;
using NotificationService.Application.Notification.Abstractions;
using NotificationService.Application.NotificationDelivery.Abstractions;
using NotificationService.Application.UserSubscription.Abstractions;
using NotificationService.Infrastructure.Caching;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetSection(RedisOptions.SectionName).GetValue<string>(nameof(RedisOptions.ConnectionString)) ?? "localhost:6379"));

        services.AddSingleton<IConnectionFactory>(_ =>
        {
            var options = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
            return new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };
        });

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
        services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

        return services;
    }
}
