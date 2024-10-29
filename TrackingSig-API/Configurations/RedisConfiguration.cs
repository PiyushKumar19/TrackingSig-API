using StackExchange.Redis;
using TrackingSig_API.Services;

namespace TrackingSig_API.Configurations;

public static class RedisConfiguration
{
    public static IServiceCollection AddRedisServices(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        var redisConfiguration = new ConfigurationOptions
        {
            EndPoints = { Environment.GetEnvironmentVariable("RedisHost")! },
            Password = Environment.GetEnvironmentVariable("RedisPassword"),
            Ssl = true,
            AbortOnConnectFail = false,
            ConnectTimeout = 10000,
            SyncTimeout = 10000,
            ConnectRetry = 3
        };

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
            var logger = sp.GetRequiredService<ILogger<RiderLocationService>>();

            if (!multiplexer.IsConnected)
            {
                logger.LogError("Failed to connect to Upstash Redis instance.");
            }

            multiplexer.ConnectionFailed += (sender, args) =>
            {
                logger.LogError(args.Exception, "Redis connection failed: {FailureType}", args.FailureType);
            };

            multiplexer.ConnectionRestored += (sender, args) =>
            {
                logger.LogInformation("Redis connection restored");
            };

            return multiplexer;
        });

        services.AddScoped<IRiderLocationService, RiderLocationService>();

        return services;
    }
}