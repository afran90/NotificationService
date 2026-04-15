namespace NotificationService.Worker;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "notifications";
    public string Queue { get; init; } = "notifications.delivery";
    public string CreatedRoutingKey { get; init; } = "notifications.created";
}
