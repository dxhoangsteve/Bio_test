#!/bin/bash

echo "🚀 Starting BioWeb Local Development"
echo "===================================="

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9.0 SDK"
    exit 1
fi

# Trust development certificates
echo "🔐 Trusting development certificates..."
dotnet dev-certs https --trust

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build solution
echo "🔨 Building solution..."
dotnet build

echo ""
echo "🌐 Starting servers..."
echo "📍 Server (API): https://localhost:7254"
echo "📍 Client (Web): https://localhost:7255"
echo ""
echo "🔑 Admin Login:"
echo "   Username: admin"
echo "   Password: BioWeb2024!"
echo ""
echo "⚠️  Press Ctrl+C to stop both servers"
echo ""

# Function to cleanup background processes
cleanup() {
    echo ""
    echo "🛑 Stopping servers..."
    kill $SERVER_PID $CLIENT_PID 2>/dev/null
    wait $SERVER_PID $CLIENT_PID 2>/dev/null
    echo "✅ Servers stopped"
    exit 0
}

# Set trap to cleanup on script exit
trap cleanup SIGINT SIGTERM

# Start server in background
echo "🖥️  Starting server..."
cd BioWeb.server
dotnet run --launch-profile https &
SERVER_PID=$!
cd ..

# Wait a bit for server to start
sleep 3

# Start client in background
echo "🌐 Starting client..."
cd BioWeb.client
dotnet run --launch-profile https &
CLIENT_PID=$!
cd ..

# Wait for both processes
wait $SERVER_PID $CLIENT_PID
