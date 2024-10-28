using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace TrackingSig_API.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly DistributedCacheEntryOptions _defaultOptions;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(
            IDistributedCache cache,
            IConnectionMultiplexer redisConnection,
            ILogger<RedisCacheService> logger,
            IOptions<DistributedCacheEntryOptions>? options = null)
        {
            _cache = cache;
            _redisConnection = redisConnection;
            _logger = logger;
            _defaultOptions = options?.Value ?? new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var jsonData = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(jsonData))
                    return default;

                return JsonSerializer.Deserialize<T>(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key {Key} from Redis cache", key);
                throw;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expirationTime.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expirationTime;
                }
                else
                {
                    options = _defaultOptions;
                }

                var jsonData = JsonSerializer.Serialize(value, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _cache.SetStringAsync(key, jsonData, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting key {Key} in Redis cache", key);
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key {Key} from Redis cache", key);
                throw;
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            try
            {
                var db = _redisConnection.GetDatabase();
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of key {Key} in Redis cache", key);
                throw;
            }
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
        {
            try
            {
                var value = await GetAsync<T>(key);
                if (value != null)
                    return value;

                value = await factory();
                if (value != null)
                    await SetAsync(key, value, expirationTime);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSet operation for key {Key} in Redis cache", key);
                throw;
            }
        }
    }
}
