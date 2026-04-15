namespace NotificationService.Worker;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";
    public int BatchSize { get; init; } = 50;
    public int PollIntervalMilliseconds { get; init; } = 1000;
    public int LockTimeoutMinutes { get; init; } = 5;
    public int MaxAttempts { get; init; } = 10;
    public int RetryDelaySeconds { get; init; } = 30;
}
