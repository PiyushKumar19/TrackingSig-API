namespace TrackingSig_API.Services;

public static class GeoDistanceCalculator
{
    public static double GetDistanceInKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers

        var lat = (lat2 - lat1) * Math.PI / 180.0;
        var lon = (lon2 - lon1) * Math.PI / 180.0;

        var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2);
        var h2 = Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) * Math.Sin(lon / 2) * Math.Sin(lon / 2);

        var h = h1 + h2;
        var c = 2 * Math.Asin(Math.Sqrt(h));

        return R * c; // Distance in kilometers
    }
}

