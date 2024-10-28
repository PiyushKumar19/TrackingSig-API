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
    private const string RiderLocationHashKey = "rider:locations";
    private const string RiderLocationGeoKey = "rider:locations:geo";
    private readonly TimeSpan _locationExpiry = TimeSpan.FromMinutes(30);

    public RiderLocationService(
        IConnectionMultiplexer redis,
        ILogger<RiderLocationService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task StoreRiderLocationAsync(string riderId, double latitude, double longitude)
    {
        try
        {
            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();

            // Store location data in a hash
            var serializedLocation = JsonSerializer.Serialize((latitude, longitude));
            await batch.HashSetAsync(RiderLocationHashKey,
                new HashEntry[] { new HashEntry(riderId, serializedLocation) });

            // Store location in geo-spatial index
            await batch.GeoAddAsync(RiderLocationGeoKey,
                new GeoEntry(longitude, latitude, riderId));

            // Set expiry for both keys
            await batch.KeyExpireAsync(RiderLocationHashKey, _locationExpiry);
            await batch.KeyExpireAsync(RiderLocationGeoKey, _locationExpiry);

            batch.Execute();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing location for rider {RiderId}", riderId);
            throw;
        }
    }

    public async Task<Dictionary<string, (double Latitude, double Longitude)>> GetAllLocationsAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            var hashEntries = await db.HashGetAllAsync(RiderLocationHashKey);

            var locations = new Dictionary<string, (double Latitude, double Longitude)>();

            foreach (var entry in hashEntries)
            {
                if (!entry.Value.IsNullOrEmpty)
                {
                    var location = JsonSerializer.Deserialize<(double Latitude, double Longitude)>(entry.Value);
                    locations[entry.Name] = location;
                }
            }

            return locations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all rider locations");
            throw;
        }
    }

    public async Task<(double Latitude, double Longitude)?> GetRiderLocationAsync(string riderId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var locationJson = await db.HashGetAsync(RiderLocationHashKey, riderId);

            if (locationJson.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<(double Latitude, double Longitude)>(locationJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location for rider {RiderId}", riderId);
            throw;
        }
    }

    public async Task<List<(string RiderId, double Latitude, double Longitude, double DistanceKm)>>
        GetNearbyRidersAsync(double latitude, double longitude, double radiusKm)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Use GeoRadiusResult instead of GeoSearchAsync
            var results = await db.GeoRadiusAsync(
                RiderLocationGeoKey,
                longitude,  // Note: Redis expects longitude first
                latitude,
                radiusKm,
                unit: GeoUnit.Kilometers,
                options: GeoRadiusOptions.Default | GeoRadiusOptions.WithCoordinates | GeoRadiusOptions.WithDistance,
                order: Order.Ascending
            );

            var nearbyRiders = new List<(string RiderId, double Latitude, double Longitude, double DistanceKm)>();

            foreach (var result in results)
            {
                if (result.Position.HasValue)
                {
                    nearbyRiders.Add((
                        result.Member.ToString(),
                        result.Position.Value.Latitude,
                        result.Position.Value.Longitude,
                        result.Distance ?? 0
                    ));
                }
            }

            return nearbyRiders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for nearby riders at ({Latitude}, {Longitude})",
                latitude, longitude);
            throw;
        }
    }
}