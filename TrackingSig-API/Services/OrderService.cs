using Microsoft.AspNetCore.SignalR;

namespace TrackingSig_API.Services
{
    public class OrderService
    {
        private readonly IHubContext<RiderHub> _riderHubContext;

        public OrderService(IHubContext<RiderHub> riderHubContext)
        {
            _riderHubContext = riderHubContext;
        }

        public async Task NotifyNearbyRidersAsync(double pickupLat, double pickupLon, double radiusInKm = 5.0)
        {
            var nearbyRiders = new List<string>();

            // Loop through the active riders and calculate the distance
            foreach (var rider in RiderHub.ActiveRiders)
            {
                var riderInfo = rider.Value;
                var distance = CalculateDistance(pickupLat, pickupLon, riderInfo.latitude, riderInfo.longitude);

                if (distance <= radiusInKm)
                {
                    nearbyRiders.Add(riderInfo.riderId);
                }
            }

            // Send notifications to all nearby riders
            foreach (var riderId in nearbyRiders)
            {
                // Assuming your riderId corresponds to a SignalR user ID
                await _riderHubContext.Clients.User(riderId).SendAsync("NewOrderNotification", pickupLat, pickupLon);
            }
        }

        // Haversine formula to calculate distance between two geocoordinates (in kilometers)
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // Radius of the earth in km
            var dLat = (lat2 - lat1) * (Math.PI / 180);
            var dLon = (lon2 - lon1) * (Math.PI / 180);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = r * c; // Distance in km
            return distance;
        }
    }

}
