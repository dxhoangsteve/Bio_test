#!/bin/bash

echo "🌐 Starting BioWeb Client (Web)"
echo "==============================="

# Trust development certificates
dotnet dev-certs https --trust

# Restore and build
dotnet restore
dotnet build

echo ""
echo "🚀 Client starting at: https://localhost:7255"
echo "📡 API Server should be running at: https://localhost:7254"
echo ""

# Start client
cd BioWeb.client
dotnet run --launch-profile https
