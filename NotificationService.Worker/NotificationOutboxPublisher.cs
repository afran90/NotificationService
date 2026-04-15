using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Domain.Notification.Enums;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;

namespace NotificationService.Worker;

public sealed class NotificationOutboxPublisher(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IOptions<OutboxOptions> outboxOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationOutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to publish notification outbox batch.");
            }

            await Task.Delay(outboxOptions.Value.PollIntervalMilliseconds, stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: rabbitMqOptions.Value.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var now = DateTime.UtcNow;
        var lockThreshold = now.AddMinutes(-outboxOptions.Value.LockTimeoutMinutes);

        var pendingMessages = await dbContext.NotificationOutboxMessages
            .Where(x =>
                (x.Status == NotificationOutboxStatus.Pending ||
                 (x.Status == NotificationOutboxStatus.Processing && x.LockedAtUtc < lockThreshold)) &&
                (!x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.CreatedAtUtc)
            .Take(outboxOptions.Value.BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return;
        }

        foreach (var message in pendingMessages)
        {
            message.Status = NotificationOutboxStatus.Processing;
            message.LockedAtUtc = now;
            message.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var message in pendingMessages)
        {
            try
            {
                var payload = Encoding.UTF8.GetBytes(message.Payload);
                await channel.BasicPublishAsync(
                    exchange: rabbitMqOptions.Value.Exchange,
                    routingKey: message.RoutingKey,
                    body: payload,
                    cancellationToken: cancellationToken);

                message.Status = NotificationOutboxStatus.Published;
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
                message.LockedAtUtc = null;
                message.NextAttemptAtUtc = null;
                message.UpdatedAtUtc = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                message.Attempts++;
                message.LastError = exception.Message;
                message.ProcessedAtUtc = null;
                message.LockedAtUtc = null;
                message.UpdatedAtUtc = DateTime.UtcNow;

                if (message.Attempts >= outboxOptions.Value.MaxAttempts)
                {
                    message.Status = NotificationOutboxStatus.Failed;
                    message.NextAttemptAtUtc = null;
                }
                else
                {
                    message.Status = NotificationOutboxStatus.Pending;
                    message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(outboxOptions.Value.RetryDelaySeconds);
                }

                logger.LogError(exception, "Failed to publish outbox message {OutboxMessageId}.", message.Id);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
