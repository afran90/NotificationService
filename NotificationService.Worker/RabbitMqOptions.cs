namespace NotificationService.Worker;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";

    public string Exchange { get; init; } = "notifications";
    public string DeadLetterExchange { get; init; } = "notifications.dlx";

    public string PushQueue { get; init; } = "notifications.delivery.push";
    public string EmailQueue { get; init; } = "notifications.delivery.email";
    public string SmsQueue { get; init; } = "notifications.delivery.sms";

    public string PushRoutingKey { get; init; } = "notifications.created.push";
    public string EmailRoutingKey { get; init; } = "notifications.created.email";
    public string SmsRoutingKey { get; init; } = "notifications.created.sms";

    public string PushDeadLetterQueue { get; init; } = "notifications.delivery.push.dlq";
    public string EmailDeadLetterQueue { get; init; } = "notifications.delivery.email.dlq";
    public string SmsDeadLetterQueue { get; init; } = "notifications.delivery.sms.dlq";

    public string PushDeadLetterRoutingKey { get; init; } = "notifications.dlq.push";
    public string EmailDeadLetterRoutingKey { get; init; } = "notifications.dlq.email";
    public string SmsDeadLetterRoutingKey { get; init; } = "notifications.dlq.sms";

    public int MaxDeliveryAttempts { get; init; } = 5;
    public int RetryDelayMilliseconds { get; init; } = 1000;
}
