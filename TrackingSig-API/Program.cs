using TrackingSig_API.Configurations;
using TrackingSig_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRedisServices(builder.Configuration);

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
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<RiderHub>("rider-hub");  // SignalR Hub endpoint
app.MapHealthChecks("/health");


app.MapControllers();

app.Run();
