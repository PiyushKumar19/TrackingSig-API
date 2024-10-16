using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace TrackingSig_API.Services;

public class RiderHub : Hub
{
    public static readonly Dictionary<string, (string riderId, double latitude, double longitude)> ActiveRiders = new();

    public override async Task OnConnectedAsync()
    {
        // Add a new connection when a rider connects (this assumes the rider sends their ID and location on connection)
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove the rider from active sessions on disconnect
        ActiveRiders.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    // Method for riders to update their location
    public async Task UpdateLocation(string riderId, double latitude, double longitude)
    {
        ActiveRiders[Context.ConnectionId] = (riderId, latitude, longitude);

        // Notify clients (optional) that the rider's location has been updated
        await Clients.All.SendAsync("RiderLocationUpdated", riderId, latitude, longitude);
    }
}
