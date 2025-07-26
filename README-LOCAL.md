# 🚀 BioWeb Local Development

## 📋 Prerequisites

- .NET 9.0 SDK
- Git
- Code editor (VS Code, Visual Studio, etc.)

## 🏃‍♂️ Quick Start

### Option 1: Run Both (Recommended)
```bash
# Make scripts executable
chmod +x run-local.sh run-server.sh run-client.sh

# Start both server and client
./run-local.sh
```

### Option 2: Run Separately
```bash
# Terminal 1 - Start Server (API)
./run-server.sh

# Terminal 2 - Start Client (Web)
./run-client.sh
```

### Option 3: Manual Start
```bash
# Restore dependencies
dotnet restore

# Terminal 1 - Server
cd BioWeb.server
dotnet run --launch-profile https

# Terminal 2 - Client  
cd BioWeb.client
dotnet run --launch-profile https
```

## 🌐 URLs

- **Client (Website)**: https://localhost:7255
- **Server (API)**: https://localhost:7254
- **Admin Panel**: https://localhost:7255/admin/login

## 🔑 Admin Credentials

- **Username**: `admin`
- **Password**: `BioWeb2024!`

## 🔧 Development

### Database
- SQLite database: `BioWeb.server/BioWeb.db`
- Auto-created on first run with seed data

### SSL Certificates
- Development certificates auto-trusted
- Located in: `BioWeb.server/certificates/`

### Hot Reload
- Both server and client support hot reload
- Changes auto-refresh in browser

## 📁 Project Structure

```
BioWeb/
├── BioWeb.server/          # ASP.NET Core API (Port 7254)
├── BioWeb.client/          # Blazor WebAssembly (Port 7255)  
├── BioWeb.Shared/          # Shared models and DTOs
├── run-local.sh           # Start both server & client
├── run-server.sh          # Start server only
└── run-client.sh          # Start client only
```

## 🛠️ Troubleshooting

### SSL Certificate Issues
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Port Already in Use
```bash
# Check what's using the ports
netstat -tlnp | grep -E ':7254|:7255'

# Kill processes if needed
sudo kill -9 <PID>
```

### Database Issues
```bash
# Delete and recreate database
rm BioWeb.server/BioWeb.db
# Restart server - database will be recreated
```

## 🎯 Features

- ✅ Personal portfolio website
- ✅ Admin dashboard
- ✅ Project management
- ✅ Blog system
- ✅ File uploads
- ✅ Responsive design
- ✅ HTTPS development
- ✅ Hot reload

## 🔄 Making Changes

1. Edit code in your preferred editor
2. Changes auto-reload in browser
3. Database changes require restart
4. New packages require `dotnet restore`

Happy coding! 🎉
