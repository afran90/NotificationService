using System.Text.Json;
using NotificationService.Application.Abstractions.Caching;
using StackExchange.Redis;

namespace NotificationService.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer multiplexer) : ICacheService
{
    private readonly IDatabase _database = multiplexer.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, payload, ttl);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value.ToString());
    }
}
