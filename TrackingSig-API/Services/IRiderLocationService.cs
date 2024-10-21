namespace TrackingSig_API.Services;

public interface IRiderLocationService
{
    void StoreRiderLocation(string riderId, double latitude, double longitude);
    Dictionary<string, (double Latitude, double Longitude)> GetAllLocations();
}
