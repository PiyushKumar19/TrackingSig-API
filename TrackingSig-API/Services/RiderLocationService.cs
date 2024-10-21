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

public class RiderLocationService : IRiderLocationService
{
    private readonly IMemoryCache _memoryCache;
    private const string RiderLocationCacheKey = "RiderLocations";

    public RiderLocationService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    // Store rider location in cache
    public void StoreRiderLocation(string riderId, double latitude, double longitude)
    {
        var riderLocations = _memoryCache.GetOrCreate(RiderLocationCacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // Keep data for 30 mins
            return new Dictionary<string, (double Latitude, double Longitude)>();
        });

        riderLocations[riderId] = (latitude, longitude);
        _memoryCache.Set(RiderLocationCacheKey, riderLocations);
    }

    // Retrieve all stored rider locations
    public Dictionary<string, (double Latitude, double Longitude)> GetAllLocations()
    {
        return _memoryCache.Get<Dictionary<string, (double Latitude, double Longitude)>>(RiderLocationCacheKey);
    }
}
