# Base image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy and restore project files
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the files and publish
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Install Redis
RUN apt-get update && \
    apt-get install -y redis-server && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy the published app
COPY --from=build /app/out .

# Copy entry script
COPY start.sh ./
RUN chmod +x start.sh

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["./start.sh"]