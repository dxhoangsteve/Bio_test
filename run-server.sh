#!/bin/bash

echo "🖥️  Starting BioWeb Server (API)"
echo "==============================="

# Trust development certificates
dotnet dev-certs https --trust

# Restore and build
dotnet restore
dotnet build

echo ""
echo "🚀 Server starting at: https://localhost:7254"
echo "🔑 Admin credentials: admin / BioWeb2024!"
echo ""

# Start server
cd BioWeb.server
dotnet run --launch-profile https
