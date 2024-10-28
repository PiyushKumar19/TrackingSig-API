using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TrackingSig_API.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddMemoryCache();  // Add memory cache service
// Add Redis configuration
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
});

// Configure cache options
builder.Services.Configure<DistributedCacheEntryOptions>(options =>
{
    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = TimeSpan.FromMinutes(2);
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Add Redis configuration (if not already added)

builder.Services.AddSingleton<IRiderLocationService, RiderLocationService>();  // Add RiderLocationService as singleton


// Optional: Add health checks for Redis
builder.Services.AddHealthChecks();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();


var app = builder.Build();

app.UseCors(cors => cors
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(origin => true));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<RiderHub>("rider-hub");  // SignalR Hub endpoint
app.MapHealthChecks("/health");


app.MapControllers();

app.Run();
