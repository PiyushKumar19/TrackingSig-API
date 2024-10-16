namespace TrackingSig_API.Services;

using StackExchange.Redis;
using System.Threading.Tasks;

//public class RedisService
//{
//    private readonly IConnectionMultiplexer _redis;

//    public RedisService(IConnectionMultiplexer redis)
//    {
//        _redis = redis;
//    }

//    public async Task AddRiderLocationAsync(string riderId, double latitude, double longitude)
//    {
//        var db = _redis.GetDatabase();
//        await db.GeoAddAsync("riders:locations", longitude, latitude, riderId);
//    }

//    public async Task SetRiderStatusAsync(string riderId, string status)
//    {
//        var db = _redis.GetDatabase();
//        await db.HashSetAsync($"rider:{riderId}", new HashEntry[]
//        {
//            new HashEntry("status", status),
//            new HashEntry("lastSeen", DateTime.UtcNow.ToString("o")) // ISO 8601 format
//        });

//        // Set expiration to remove inactive riders
//        await db.KeyExpireAsync($"rider:{riderId}", TimeSpan.FromMinutes(30));
//    }

//    public async Task<GeoPosition?> GetRiderLocationAsync(string riderId)
//    {
//        var db = _redis.GetDatabase();
//        return await db.GeoPositionAsync("riders:locations", riderId);
//    }

//    public async Task<GeoRadiusResult[]> GetNearbyRidersAsync(double latitude, double longitude, double radiusInKm)
//    {
//        var db = _redis.GetDatabase();

//        // This returns an array of GeoRadiusResult
//        return await db.GeoRadiusAsync("riders:locations", longitude, latitude, radiusInKm, GeoUnit.Kilometers);
//    }

//}

public class RiderService
{
    private readonly IDatabase _redisDatabase;
    private readonly ILogger<RiderService> _logger;

    public RiderService(IConnectionMultiplexer redis, ILogger<RiderService> logger)
    {
        _redisDatabase = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<IEnumerable<GeoRadiusResult>> GetNearbyRidersAsync(double latitude, double longitude, double radiusInKm)
    {
        try
        {
            var nearbyRiders = await _redisDatabase.GeoRadiusAsync("riders:locations", longitude, latitude, radiusInKm, GeoUnit.Kilometers);
            _logger.LogInformation($"Fetched {nearbyRiders.Length} nearby riders.");
            return nearbyRiders;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving nearby riders: {ex.Message}");
            throw;
        }
    }
}
