namespace TrackingSig_API.Services;

using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

//public class PickupService
//{
//    private readonly RiderLocationService _riderLocationService;
//    private readonly IHubContext<RiderHub> _hubContext;

//    public PickupService(RiderLocationService riderLocationService, IHubContext<RiderHub> hubContext)
//    {
//        _riderLocationService = riderLocationService;
//        _hubContext = hubContext;
//    }

//    // Notify riders within 5 km of the pickup location
//    public async Task NotifyRidersNearPickup(double pickupLat, double pickupLon)
//    {
//        // Get all active riders and their locations
//        var riders = _riderLocationService.GetActiveRiders();

//        // Loop through riders and check if they're within 5 km
//        foreach (var rider in riders)
//        {
//            var riderId = rider.Key;
//            var (riderLat, riderLon) = rider.Value;

//            // Calculate distance between rider and pickup location
//            var distance = GeoDistanceCalculator.GetDistanceInKm(pickupLat, pickupLon, riderLat, riderLon);

//            // If within 5 km, send a notification (using SignalR)
//            if (distance <= 5)
//            {
//                await _hubContext.Clients.User(riderId).SendAsync("ReceivePickupNotification", "New pickup order nearby!");
//            }
//        }
//    }
//}

