using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace TrackingSig_API.Services;

//public class RiderHub : Hub
//{
//    public static readonly Dictionary<string, (string riderId, double latitude, double longitude)> ActiveRiders = new();


//    private readonly RiderLocationService _riderLocationService;

//    // Inject RiderLocationService via constructor
//    public RiderHub(RiderLocationService riderLocationService)
//    {
//        _riderLocationService = riderLocationService;
//    }


//    public override async Task OnConnectedAsync()
//    {
//        // Add a new connection when a rider connects (this assumes the rider sends their ID and location on connection)
//        await base.OnConnectedAsync();
//    }

//    public override async Task OnDisconnectedAsync(Exception? exception)
//    {
//        // Remove the rider from active sessions on disconnect
//        ActiveRiders.Remove(Context.ConnectionId);
//        await base.OnDisconnectedAsync(exception);
//    }

//    // Method for riders to update their location
//    public async Task UpdateLocation(string riderId, double latitude, double longitude)
//    {
//        ActiveRiders[Context.ConnectionId] = (riderId, latitude, longitude);

//        // Notify clients (optional) that the rider's location has been updated
//        await Clients.All.SendAsync("RiderLocationUpdated", riderId, latitude, longitude);
//    }

//    // Method for rider to send geolocation data
//    public async Task SendLocation(string userId, double latitude, double longitude)
//    {
//        // Use a service to handle saving data into cache
//        await _riderLocationService.StoreRiderLocation(userId, latitude, longitude);
//    }
//}



//public class RiderHub : Hub
//{
//    private readonly RiderLocationService _riderLocationService;
//    private readonly ILogger<RiderHub> _logger;

//    public RiderHub(RiderLocationService riderLocationService, ILogger<RiderHub> logger)
//    {
//        _riderLocationService = riderLocationService;
//        _logger = logger;
//    }

//    public override async Task OnConnectedAsync()
//    {
//        // Add a new connection when a rider connects (this assumes the rider sends their ID and location on connection)
//        Console.WriteLine("User connected!");
//        _logger.LogInformation("---> User connected!");
//        await base.OnConnectedAsync();
//    }

//    public override async Task OnDisconnectedAsync(Exception? exception)
//    {
//        // Remove the rider from active sessions on disconnect
//        await base.OnDisconnectedAsync(exception);
//    }

//    // Method to update rider location (invoked from clients)
//    public async Task UpdateRiderLocation(string riderId, double latitude, double longitude)
//    {
//        // Store the rider location in cache
//        _riderLocationService.StoreRiderLocation(riderId, latitude, longitude);

//        // Log and send confirmation back to the caller
//        _logger.LogInformation($"Rider {riderId} updated location: Lat={latitude}, Lon={longitude}");

//        await Clients.Caller.SendAsync("LocationUpdated", $"Rider {riderId} location stored successfully.");
//    }

//    // Method to get rider location
//    public object GetRiderLocation(string riderId)
//    {
//        var location = _riderLocationService.GetRiderLocation(riderId);
//        if (location != null)
//        {
//            return location;
//        }

//        return "Location not found.";
//    }
//}


using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class RiderHub : Hub
{
    private readonly IRiderLocationService _riderLocationService;

    public RiderHub(IRiderLocationService riderLocationService)
    {
        _riderLocationService = riderLocationService;
    }

    public override async Task OnConnectedAsync()
    {
        // This is where you should hit the breakpoint
        Console.WriteLine("Client connected.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Handle disconnection if needed
        Console.WriteLine("Client disconnected.");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UpdateRiderLocation(string riderId, double latitude, double longitude)
    {
        // Store rider location in the in-memory cache
        _riderLocationService.StoreRiderLocation(riderId, latitude, longitude);

        // Log the operation (optional for debugging)
        Console.WriteLine($"Rider {riderId} updated location: Lat={latitude}, Lon={longitude}");

        // Send confirmation back to the client (optional)
        await Clients.Caller.SendAsync("LocationUpdated", $"Location data for rider {riderId} stored successfully.");
    }
}
