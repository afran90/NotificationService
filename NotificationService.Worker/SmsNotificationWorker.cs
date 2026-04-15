using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Domain.Notification.Enums;
using RabbitMQ.Client;

namespace NotificationService.Worker;

public sealed class SmsNotificationWorker(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<SmsNotificationWorker> logger)
    : NotificationDeliveryWorkerBase(connectionFactory, options, scopeFactory, logger)
{
    protected override NotificationType SupportedType => NotificationType.SMS;
    protected override string Destination => "sms";

    protected override string GetQueueName(RabbitMqOptions options) => options.SmsQueue;
    protected override string GetRoutingKey(RabbitMqOptions options) => options.SmsRoutingKey;
    protected override string GetDeadLetterQueueName(RabbitMqOptions options) => options.SmsDeadLetterQueue;
    protected override string GetDeadLetterRoutingKey(RabbitMqOptions options) => options.SmsDeadLetterRoutingKey;

    protected override Task SendThroughChannelAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("SMS notification sent to user {UserId}.", message.UserId);
        return Task.CompletedTask;
    }
}
