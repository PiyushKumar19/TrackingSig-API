#!/bin/bash

# Start Redis server in the background
redis-server --daemonize yes --bind 127.0.0.1

# Start the .NET application
dotnet TrackingSig.dll