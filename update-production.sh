#!/bin/bash

# ===================================================================
# BioWeb Production Update Script
# ===================================================================
# Script cập nhật code cho production server
# Chạy: sudo ./update-production.sh
# ===================================================================

set -e

INSTALL_DIR="/var/www/bioweb"
SERVICE_USER="bioweb"

echo "🔄 Updating BioWeb Production..."

# Backup database trước khi update
echo "📦 Creating database backup..."
cp $INSTALL_DIR/data/BioWeb.db $INSTALL_DIR/backups/pre-update-backup-$(date +%Y%m%d_%H%M%S).db
echo "✅ Database backed up"

# Stop service
echo "⏹️  Stopping BioWeb server..."
systemctl stop bioweb-server

# Update source code
echo "📥 Updating source code..."
cd $INSTALL_DIR/source
git pull origin main

# Build new version
echo "🔨 Building updated version..."
dotnet restore
dotnet publish BioWeb.server/BioWeb.server.csproj -c Release -o $INSTALL_DIR/server
dotnet publish BioWeb.client/BioWeb.client.csproj -c Release -o $INSTALL_DIR/client

# Set permissions
echo "🔐 Setting permissions..."
chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR/server
chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR/client

# Start service
echo "🚀 Starting BioWeb server..."
systemctl start bioweb-server

# Reload Nginx
echo "🌐 Reloading Nginx..."
systemctl reload nginx

# Check status
echo "🔍 Checking service status..."
systemctl status bioweb-server --no-pager

echo ""
echo "✅ Update completed successfully!"
echo "🌐 Website: https://yourdomain.com"
echo "📋 Check logs: journalctl -u bioweb-server -f"
