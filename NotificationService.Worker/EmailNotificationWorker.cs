using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Domain.Notification.Enums;
using RabbitMQ.Client;

namespace NotificationService.Worker;

public sealed class EmailNotificationWorker(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailNotificationWorker> logger)
    : NotificationDeliveryWorkerBase(connectionFactory, options, scopeFactory, logger)
{
    protected override NotificationType SupportedType => NotificationType.Email;
    protected override string Destination => "email";

    protected override string GetQueueName(RabbitMqOptions options) => options.EmailQueue;
    protected override string GetRoutingKey(RabbitMqOptions options) => options.EmailRoutingKey;
    protected override string GetDeadLetterQueueName(RabbitMqOptions options) => options.EmailDeadLetterQueue;
    protected override string GetDeadLetterRoutingKey(RabbitMqOptions options) => options.EmailDeadLetterRoutingKey;

    protected override Task SendThroughChannelAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email notification sent to user {UserId}.", message.UserId);
        return Task.CompletedTask;
    }
}
