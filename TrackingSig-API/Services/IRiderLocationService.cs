using StackExchange.Redis;

namespace TrackingSig_API.Services;

//public interface IRiderLocationService
//{
//    void StoreRiderLocation(string riderId, double latitude, double longitude);
//    Dictionary<string, (double Latitude, double Longitude)> GetAllLocations();
//}

public interface IRiderLocationService
{
    //Task StoreRiderLocationAsync(string riderId, double latitude, double longitude);
    //Task<Dictionary<string, (double Latitude, double Longitude)>> GetAllLocationsAsync();
    //Task<(double Latitude, double Longitude)?> GetRiderLocationAsync(string riderId);
    //Task<List<(string RiderId, double Latitude, double Longitude, double DistanceKm)>> GetNearbyRidersAsync(
    //    double latitude,
    //    double longitude,
    //    double radiusKm);

    Task StoreRiderLocationAsync(string riderId, double latitude, double longitude);
    Task<Location> GetRiderLocationAsync(string riderId);
    Task<IEnumerable<Location>> GetAllRiderLocationsAsync();
}
