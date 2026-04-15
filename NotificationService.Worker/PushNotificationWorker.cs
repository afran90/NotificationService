using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Domain.Notification.Enums;
using RabbitMQ.Client;

namespace NotificationService.Worker;

public sealed class PushNotificationWorker(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<PushNotificationWorker> logger)
    : NotificationDeliveryWorkerBase(connectionFactory, options, scopeFactory, logger)
{
    protected override NotificationType SupportedType => NotificationType.Push;
    protected override string Destination => "push";

    protected override string GetQueueName(RabbitMqOptions options) => options.PushQueue;
    protected override string GetRoutingKey(RabbitMqOptions options) => options.PushRoutingKey;
    protected override string GetDeadLetterQueueName(RabbitMqOptions options) => options.PushDeadLetterQueue;
    protected override string GetDeadLetterRoutingKey(RabbitMqOptions options) => options.PushDeadLetterRoutingKey;

    protected override Task SendThroughChannelAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Push notification sent to user {UserId}.", message.UserId);
        return Task.CompletedTask;
    }
}
