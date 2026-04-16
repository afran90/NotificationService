using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Application.NotificationDelivery.Services;
using NotificationService.Domain.Notification.Enums;
using NotificationService.Domain.NotificationDelivery.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Worker;

public abstract class NotificationDeliveryWorkerBase(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected ILogger Logger { get; } = logger;

    protected abstract NotificationType SupportedType { get; }
    protected abstract string Destination { get; }
    protected abstract string GetQueueName(RabbitMqOptions options);
    protected abstract string GetRoutingKey(RabbitMqOptions options);
    protected abstract string GetDeadLetterQueueName(RabbitMqOptions options);
    protected abstract string GetDeadLetterRoutingKey(RabbitMqOptions options);

    protected abstract Task SendThroughChannelAsync(NotificationMessage message, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMq = options.Value;

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken: stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var queue = GetQueueName(rabbitMq);
        var routingKey = GetRoutingKey(rabbitMq);
        var deadLetterQueue = GetDeadLetterQueueName(rabbitMq);
        var deadLetterRoutingKey = GetDeadLetterRoutingKey(rabbitMq);

        await channel.ExchangeDeclareAsync(
            exchange: rabbitMq.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: rabbitMq.DeadLetterExchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: queue,
            exchange: rabbitMq.Exchange,
            routingKey: routingKey,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: deadLetterQueue,
            exchange: rabbitMq.DeadLetterExchange,
            routingKey: deadLetterRoutingKey,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            NotificationMessage? message = null;

            try
            {
                message = JsonSerializer.Deserialize<NotificationMessage>(eventArgs.Body.ToArray(), SerializerOptions);
                if (message is null)
                {
                    Logger.LogWarning("Could not deserialize RabbitMQ message. Sending to DLQ.");
                    await PublishToDeadLetterAsync(channel, rabbitMq, deadLetterRoutingKey, eventArgs, stoppingToken);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                if (message.Type != SupportedType)
                {
                    Logger.LogWarning(
                        "Worker received unsupported type. Expected {ExpectedType}, got {ActualType}. Sending to DLQ.",
                        SupportedType,
                        message.Type);

                    await PublishToDeadLetterAsync(channel, rabbitMq, deadLetterRoutingKey, eventArgs, stoppingToken);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                await SendThroughChannelAsync(message, stoppingToken);
                await PersistDeliveryResultAsync(message.NotificationId, DeliveryStatus.Sent, reason: null, stoppingToken);

                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);

                Logger.LogInformation(
                    "Delivered notification {NotificationId} to {Destination}.",
                    message.NotificationId,
                    Destination);
            }
            catch (Exception exception)
            {
                if (message is null)
                {
                    await PublishToDeadLetterAsync(channel, rabbitMq, deadLetterRoutingKey, eventArgs, stoppingToken);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    Logger.LogError(exception, "Failed message with empty payload was sent to DLQ.");
                    return;
                }

                var attempt = await GetNextAttemptAsync(message.NotificationId, stoppingToken);

                if (attempt >= rabbitMq.MaxDeliveryAttempts)
                {
                    await PersistDeliveryResultAsync(message.NotificationId, DeliveryStatus.Failed, exception.Message, stoppingToken);
                    await PublishToDeadLetterAsync(channel, rabbitMq, deadLetterRoutingKey, eventArgs, stoppingToken);

                    Logger.LogError(
                        exception,
                        "Delivery failed for notification {NotificationId}. Sent to DLQ after {Attempt} attempts.",
                        message.NotificationId,
                        attempt);
                }
                else
                {
                    await PersistDeliveryResultAsync(message.NotificationId, DeliveryStatus.Retrying, exception.Message, stoppingToken);

                    if (rabbitMq.RetryDelayMilliseconds > 0)
                    {
                        await Task.Delay(rabbitMq.RetryDelayMilliseconds, stoppingToken);
                    }

                    await channel.BasicPublishAsync(
                        exchange: rabbitMq.Exchange,
                        routingKey: routingKey,
                        body: eventArgs.Body,
                        cancellationToken: stoppingToken);

                    Logger.LogWarning(
                        exception,
                        "Delivery failed for notification {NotificationId}. Requeued attempt {Attempt}/{MaxAttempts}.",
                        message.NotificationId,
                        attempt,
                        rabbitMq.MaxDeliveryAttempts);
                }

                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<int> GetNextAttemptAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var deliveryService = scope.ServiceProvider.GetRequiredService<INotificationDeliveryService>();

        return await deliveryService.GetNextAttemptAsync(notificationId, Destination, cancellationToken);
    }

    private async Task PersistDeliveryResultAsync(Guid notificationId, DeliveryStatus status, string? reason, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var deliveryService = scope.ServiceProvider.GetRequiredService<INotificationDeliveryService>();

        await deliveryService.RecordResultAsync(notificationId, Destination, status, reason, cancellationToken);
    }

    private static Task PublishToDeadLetterAsync(
        IChannel channel,
        RabbitMqOptions rabbitMq,
        string deadLetterRoutingKey,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        return channel.BasicPublishAsync(
            exchange: rabbitMq.DeadLetterExchange,
            routingKey: deadLetterRoutingKey,
            body: eventArgs.Body,
            cancellationToken: cancellationToken).AsTask();
    }
}
