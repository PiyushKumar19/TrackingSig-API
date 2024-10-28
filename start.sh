#!/bin/bash

# Start Redis server
redis-server /etc/redis/redis.conf

# Wait for Redis to be ready
until redis-cli ping
do
    echo "Waiting for Redis to be ready..."
    sleep 1
done

# Start the .NET application
dotnet TrackingSig-API.dll