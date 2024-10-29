namespace TrackingSig_API.Services;

using Microsoft.Extensions.Caching.Memory;

//public class RiderLocationService
//{
//    private readonly IMemoryCache _cache;
//    private readonly ILogger<RiderLocationService> _logger;

//    public RiderLocationService(IMemoryCache cache, ILogger<RiderLocationService> logger)
//    {
//        _cache = cache;
//        _logger = logger;
//    }

//    // Store rider's location
//    public void StoreRiderLocation(string riderId, double latitude, double longitude)
//    {
//        var cacheKey = $"RiderLocation_{riderId}";
//        var locationData = new { Latitude = latitude, Longitude = longitude };

//        // Store data with expiration (e.g., 30 minutes)
//        _cache.Set(cacheKey, locationData, TimeSpan.FromMinutes(30));

//        _logger.LogInformation($"Stored location for rider {riderId}: Lat={latitude}, Lon={longitude}");
//    }

//    // Retrieve rider's location
//    public object GetRiderLocation(string riderId)
//    {
//        var cacheKey = $"RiderLocation_{riderId}";
//        if (_cache.TryGetValue(cacheKey, out object locationData))
//        {
//            _logger.LogInformation($"Retrieved location for rider {riderId}");
//            return locationData;
//        }

//        _logger.LogWarning($"No location found for rider {riderId}");
//        return null;
//    }
//}


using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

//public class RiderLocationService : IRiderLocationService
//{
//    private readonly IMemoryCache _memoryCache;
//    private const string RiderLocationCacheKey = "RiderLocations";

//    public RiderLocationService(IMemoryCache memoryCache)
//    {
//        _memoryCache = memoryCache;
//    }

//    // Store rider location in cache
//    public void StoreRiderLocation(string riderId, double latitude, double longitude)
//    {
//        var riderLocations = _memoryCache.GetOrCreate(RiderLocationCacheKey, entry =>
//        {
//            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // Keep data for 30 mins
//            return new Dictionary<string, (double Latitude, double Longitude)>();
//        });

//        riderLocations[riderId] = (latitude, longitude);
//        _memoryCache.Set(RiderLocationCacheKey, riderLocations);
//    }

//    // Retrieve all stored rider locations
//    public Dictionary<string, (double Latitude, double Longitude)> GetAllLocations()
//    {
//        return _memoryCache.Get<Dictionary<string, (double Latitude, double Longitude)>>(RiderLocationCacheKey);
//    }
//}


using StackExchange.Redis;
using System.Text.Json;

public class RiderLocationService : IRiderLocationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RiderLocationService> _logger;
    private readonly TimeSpan _locationExpiry = TimeSpan.FromHours(24); // Or your preferred expiry time
    private const string RiderLocationHashKey = "rider:locations";
    private const string RiderLocationGeoKey = "rider:locations:geo";

    public RiderLocationService(IConnectionMultiplexer redis, ILogger<RiderLocationService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StoreRiderLocationAsync(string riderId, double latitude, double longitude)
    {
        if (string.IsNullOrEmpty(riderId))
            throw new ArgumentNullException(nameof(riderId));

        // Retry logic configuration
        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Check if connection is available
                if (!_redis.IsConnected)
                {
                    _logger.LogWarning("Redis connection is not available. Attempting to reconnect...");
                    await _redis.ConfigureAsync();
                }

                var db = _redis.GetDatabase();

                // Serialize the location
                var location = new Location { Latitude = latitude, Longitude = longitude };
                var serializedLocation = JsonSerializer.Serialize(location);

                // Execute commands individually with error handling
                await ExecuteRedisOperationsAsync(db, riderId, latitude, longitude, serializedLocation);

                // If successful, break the retry loop
                break;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error on attempt {Attempt} for rider {RiderId}", attempt + 1, riderId);

                if (attempt == maxRetries)
                {
                    throw new RedisConnectionException(ConnectionFailureType.InternalFailure, ex.Message);
                }

                await Task.Delay(retryDelay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing location for rider {RiderId} on attempt {Attempt}", riderId, attempt + 1);
                throw;
            }
        }
    }

    public async Task<Location> GetRiderLocationAsync(string riderId)
    {
        if (string.IsNullOrEmpty(riderId))
            throw new ArgumentNullException(nameof(riderId));

        try
        {
            var db = _redis.GetDatabase();
            var serializedLocation = await db.HashGetAsync(RiderLocationHashKey, riderId);
            if (!string.IsNullOrEmpty(serializedLocation))
            {
                return JsonSerializer.Deserialize<Location>(serializedLocation);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location for rider {RiderId}", riderId);
            throw;
        }
    }

    public async Task<IEnumerable<Location>> GetAllRiderLocationsAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            var allRiderLocations = await db.HashGetAllAsync(RiderLocationHashKey);
            return allRiderLocations.Select(x => JsonSerializer.Deserialize<Location>(x.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all rider locations");
            throw;
        }
    }

    private async Task ExecuteRedisOperationsAsync(IDatabase db, string riderId, double latitude, double longitude, string serializedLocation)
    {
        // Execute commands individually instead of using batch
        var tasks = new List<Task>
        {
            // Store the location hash
            db.HashSetAsync(RiderLocationHashKey,
                new HashEntry[] { new HashEntry(riderId, serializedLocation) }),

            // Store the geo location
            db.GeoAddAsync(RiderLocationGeoKey,
                new GeoEntry(longitude, latitude, riderId)),

            // Set expiration for both keys
            db.KeyExpireAsync(RiderLocationHashKey, _locationExpiry),
            db.KeyExpireAsync(RiderLocationGeoKey, _locationExpiry)
        };

        await Task.WhenAll(tasks);
    }
}