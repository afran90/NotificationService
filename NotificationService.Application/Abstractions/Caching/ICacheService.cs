namespace NotificationService.Application.Abstractions.Caching;

public interface ICacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
}
