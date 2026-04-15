using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Application.NotificationDelivery.Abstractions;
using NotificationService.Domain.Notification.Enums;
using NotificationService.Domain.NotificationDelivery.Enums;
using NotificationDeliveryEntity = NotificationService.Domain.NotificationDelivery.Entities.NotificationDelivery;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Worker;

public sealed class NotificationConsumer(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken: stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: options.Value.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: options.Value.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: options.Value.Queue,
            exchange: options.Value.Exchange,
            routingKey: options.Value.CreatedRoutingKey,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<NotificationMessage>(eventArgs.Body.ToArray(), SerializerOptions);
                if (message is null)
                {
                    logger.LogWarning("RabbitMQ notification message could not be deserialized.");
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
                }

                await using var scope = scopeFactory.CreateAsyncScope();
                var deliveryRepository = scope.ServiceProvider.GetRequiredService<INotificationDeliveryRepository>();

                var delivery = new NotificationDeliveryEntity
                {
                    NotificationId = message.NotificationId,
                    Destination = GetDestination(message.Type),
                    Status = DeliveryStatus.Sent,
                    DeliveredAtUtc = DateTime.UtcNow
                };

                await deliveryRepository.AddAsync(delivery, stoppingToken);

                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);

                logger.LogInformation(
                    "Processed notification {NotificationId} for user {UserId} via {Destination}.",
                    message.NotificationId,
                    message.UserId,
                    delivery.Destination);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process RabbitMQ notification message.");
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: options.Value.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private static string GetDestination(NotificationType type)
    {
        return type switch
        {
            NotificationType.Push => "push",
            NotificationType.Email => "email",
            NotificationType.SMS => "sms",
            NotificationType.InApp => "in-app",
            _ => "unknown"
        };
    }
}
