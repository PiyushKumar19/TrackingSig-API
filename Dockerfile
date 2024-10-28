#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Remove USER app as we need root to install Redis
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 6379

# Install Redis
RUN apt-get update && \
    apt-get install -y redis-server && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TrackingSig-API/TrackingSig-API.csproj", "TrackingSig-API/"]
RUN dotnet restore "./TrackingSig-API/TrackingSig-API.csproj"
COPY . .
WORKDIR "/src/TrackingSig-API"
RUN dotnet build "./TrackingSig-API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrackingSig-API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy Redis configuration
COPY redis.conf /etc/redis/redis.conf

# Create start script
COPY start.sh /app/start.sh
RUN chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]