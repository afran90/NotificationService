namespace NotificationService.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string route, T message, CancellationToken cancellationToken = default);
}
