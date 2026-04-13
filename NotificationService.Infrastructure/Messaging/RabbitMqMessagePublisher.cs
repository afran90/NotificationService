using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationService.Application.Abstractions.Messaging;
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging;

public class RabbitMqMessagePublisher(
    IConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> options) : IMessagePublisher
{
    public async Task PublishAsync<T>(string route, T message, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(options.Value.Exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(exchange: options.Value.Exchange, routingKey: route, body: payload, cancellationToken: cancellationToken);
    }
}
