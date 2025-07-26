#!/bin/bash

# ===================================================================
# BioWeb Production Deployment Script
# ===================================================================
# Script tự động deploy BioWeb lên production server
# Chạy với quyền root: ./deploy-production.sh (đã login root)
# ===================================================================

set -e  # Exit on any error

# Function để chạy lệnh (không cần sudo vì đã chạy với quyền root)
auto_sudo() {
    "$@"
}

# ===================================================================
# QUAN TRỌNG: CẤU HÌNH CẦN SỬA TRƯỚC KHI CHẠY
# ===================================================================
# TODO: Sửa các biến sau theo server của bạn:
DOMAIN="dxhoang.site"                    # ⚠️  SỬA DOMAIN CỦA BẠN
EMAIL="sterbe2k4@gmail.com"               # ⚠️  SỬA EMAIL CỦA BẠN
JWT_SECRET="hLyhJGGdiaf83JyhdH" # ⚠️  SỬA JWT SECRET KEY
ADMIN_USERNAME="dxhoang031"                     # ⚠️  SỬA ADMIN USERNAME
ADMIN_PASSWORD="02052004*"                       # ⚠️  SỬA ADMIN PASSWORD

# Đường dẫn cài đặt
INSTALL_DIR="/var/www/bioweb"
SERVICE_USER="bioweb"
# Thư mục source hiện tại (nơi chứa script này)
CURRENT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "====================================================================="
echo "🚀 BioWeb Production Deployment Starting..."
echo "====================================================================="
echo "Domain: $DOMAIN"
echo "Install Directory: $INSTALL_DIR"
echo "Service User: $SERVICE_USER"
echo ""

# ===================================================================
# BƯỚC 1: KIỂM TRA VÀ CÀI ĐẶT DEPENDENCIES
# ===================================================================
echo "📦 Installing system dependencies..."

# Kiểm tra quyền root
echo "🔐 Checking root privileges..."
if [ "$EUID" -ne 0 ]; then
    echo "❌ This script must be run as root. Please run with: sudo ./deploy-production.sh"
    exit 1
fi
echo "✅ Running as root"

# Update system
apt update && apt upgrade -y

# Install essential packages
apt install -y curl wget git nginx sqlite3 ufw certbot python3-certbot-nginx

# Install .NET 9.0
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET 9.0..."
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt update
    apt install -y dotnet-sdk-9.0
fi

# Install Entity Framework tools
echo "Installing EF Core tools..."
dotnet tool install --global dotnet-ef || dotnet tool update --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"

echo "✅ Dependencies installed successfully"

# ===================================================================
# BƯỚC 2: TẠO USER VÀ THƯ MỤC
# ===================================================================
echo "👤 Setting up user and directories..."

# Tạo user cho service (nếu chưa có)
if ! id "$SERVICE_USER" &>/dev/null; then
    useradd -r -s /bin/false -d $INSTALL_DIR $SERVICE_USER
fi

# Tạo thư mục và set permissions
mkdir -p $INSTALL_DIR
mkdir -p $INSTALL_DIR/data
mkdir -p $INSTALL_DIR/certificates
mkdir -p $INSTALL_DIR/logs
mkdir -p $INSTALL_DIR/backups

echo "✅ User and directories created"

# ===================================================================
# BƯỚC 3: COPY SOURCE CODE TỪ THỦ MỤC HIỆN TẠI
# ===================================================================
echo "📥 Copying source code from current directory..."

# Tạo thư mục source nếu chưa có
mkdir -p $INSTALL_DIR/source

# Copy toàn bộ source code từ thư mục hiện tại
echo "Copying files from $CURRENT_DIR to $INSTALL_DIR/source..."
rsync -av --exclude='.git' --exclude='*.sh' --exclude='*.md' "$CURRENT_DIR/" "$INSTALL_DIR/source/"

cd $INSTALL_DIR/source

echo "✅ Source code copied successfully"

# ===================================================================
# BƯỚC 4: BUILD PROJECT
# ===================================================================
echo "🔨 Building project..."

# Restore packages
echo "Restoring packages..."
dotnet restore || {
    echo "Error: Failed to restore packages"
    exit 1
}

# Build server
echo "Building server..."
dotnet publish BioWeb.server/BioWeb.server.csproj -c Release -o $INSTALL_DIR/server || {
    echo "Error: Failed to build server"
    exit 1
}

# Build client
echo "Building client..."
dotnet publish BioWeb.client/BioWeb.client.csproj -c Release -o $INSTALL_DIR/client || {
    echo "Error: Failed to build client"
    exit 1
}

echo "✅ Project built successfully"

# ===================================================================
# BƯỚC 5: CẤU HÌNH DATABASE VÀ MIGRATION
# ===================================================================
echo "🗄️  Setting up database..."

# Tạo database directory nếu chưa có
mkdir -p $INSTALL_DIR/data

# Backup database cũ nếu có
if [ -f "$INSTALL_DIR/data/BioWeb.db" ]; then
    echo "⚠️  CẢNH BÁO: Tìm thấy database cũ. Backup trước khi cập nhật..."
    cp $INSTALL_DIR/data/BioWeb.db $INSTALL_DIR/backups/BioWeb.db.backup.$(date +%Y%m%d_%H%M%S)
    echo "✅ Database backed up"
fi

# Chạy database migration
echo "Running database migrations..."
cd $INSTALL_DIR/server
export ConnectionStrings__DefaultConnection="Data Source=$INSTALL_DIR/data/BioWeb.db"

# Kiểm tra xem có file dll không
if [ ! -f "BioWeb.server.dll" ]; then
    echo "Error: BioWeb.server.dll not found in $INSTALL_DIR/server"
    exit 1
fi

# Tạo database và chạy migrations
echo "Starting migration process..."
timeout 30 dotnet BioWeb.server.dll --migrate-database || {
    echo "Warning: Migration timeout or failed, continuing..."
}

# Cập nhật admin user trong database với password hash
echo "👤 Updating admin user credentials..."

# Tạo script C# tạm để hash password theo cách của BioWeb
cat > /tmp/hash_password.cs << 'EOF'
using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordHasher
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run password");
            return;
        }

        string password = args[0];
        string hashedPassword = HashPassword(password);
        Console.WriteLine(hashedPassword);
    }

    public static string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // Chuyển password thành byte array
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Chuyển byte array thành string hex (giống BioWeb PasswordService)
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
EOF

# Compile và chạy để hash password
cd /tmp
dotnet new console --force --name HashPassword
cp hash_password.cs HashPassword/Program.cs
cd HashPassword
HASHED_PASSWORD=$(dotnet run "$ADMIN_PASSWORD")
cd $INSTALL_DIR/server

# Cập nhật database với password đã hash
sqlite3 $INSTALL_DIR/data/BioWeb.db << EOF
UPDATE AdminUsers SET
    Username = '$ADMIN_USERNAME',
    PasswordHash = '$HASHED_PASSWORD'
WHERE AdminID = 1;
EOF

# Cleanup
rm -rf /tmp/HashPassword /tmp/hash_password.cs

echo "✅ Admin credentials updated: $ADMIN_USERNAME / $ADMIN_PASSWORD"

echo "✅ Database setup completed"

# ===================================================================
# BƯỚC 6: CẤU HÌNH SSL CERTIFICATES
# ===================================================================
echo "Setting up SSL certificates..."

# Tạo self-signed certificate cho development
if [ ! -f "$INSTALL_DIR/certificates/server.crt" ] || [ ! -f "$INSTALL_DIR/certificates/server.key" ]; then
    echo "Creating temporary self-signed certificate..."

    # Tạo certificate và key
    openssl req -x509 -newkey rsa:2048 -keyout $INSTALL_DIR/certificates/server.key \
        -out $INSTALL_DIR/certificates/server.crt -days 365 -nodes \
        -subj "/C=VN/ST=HCM/L=HCM/O=BioWeb/CN=$DOMAIN" || {
        echo "Error: Failed to create SSL certificate"
        exit 1
    }

    # Set proper permissions
    chmod 600 $INSTALL_DIR/certificates/server.key
    chmod 644 $INSTALL_DIR/certificates/server.crt

    echo "SSL certificates created successfully"
else
    echo "SSL certificates already exist"
fi

echo "SSL certificates ready"

# ===================================================================
# BƯỚC 7: CẤU HÌNH NGINX REVERSE PROXY
# ===================================================================
echo "🌐 Configuring Nginx..."

# Backup nginx config nếu có
if [ -f "/etc/nginx/sites-available/$DOMAIN" ]; then
    cp /etc/nginx/sites-available/$DOMAIN /etc/nginx/sites-available/$DOMAIN.backup.$(date +%Y%m%d_%H%M%S)
fi

# Tạo Nginx config
cat > /etc/nginx/sites-available/$DOMAIN << EOF
# BioWeb Nginx Configuration
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;

    # Redirect HTTP to HTTPS
    return 301 https://\$server_name\$request_uri;
}

server {
    listen 443 ssl http2;
    server_name $DOMAIN www.$DOMAIN;

    # SSL Configuration - SẼ ĐƯỢC CẬP NHẬT BỞI CERTBOT
    ssl_certificate $INSTALL_DIR/certificates/server.crt;
    ssl_certificate_key $INSTALL_DIR/certificates/server.key;

    # Security headers
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    add_header X-XSS-Protection "1; mode=block";

    # Client static files
    location / {
        root $INSTALL_DIR/client/wwwroot;
        try_files \$uri \$uri/ /index.html;

        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }

    # API proxy to backend
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }

    # File uploads
    location /uploads/ {
        alias $INSTALL_DIR/server/wwwroot/uploads/;
        expires 1d;
    }
}
EOF

# Enable site
ln -sf /etc/nginx/sites-available/$DOMAIN /etc/nginx/sites-enabled/
rm -f /etc/nginx/sites-enabled/default

# Test nginx config
nginx -t

echo "✅ Nginx configured"

# ===================================================================
# BƯỚC 8: TẠO SYSTEMD SERVICES
# ===================================================================
echo "⚙️  Creating systemd services..."

# BioWeb Server Service
cat > /etc/systemd/system/bioweb-server.service << EOF
[Unit]
Description=BioWeb Server API
After=network.target

[Service]
Type=notify
User=$SERVICE_USER
WorkingDirectory=$INSTALL_DIR/server
ExecStart=/usr/bin/dotnet BioWeb.server.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=bioweb-server
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=ProductionDomain=$DOMAIN
Environment=ConnectionStrings__DefaultConnection="Data Source=$INSTALL_DIR/data/BioWeb.db"

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd và enable services
systemctl daemon-reload
systemctl enable bioweb-server

echo "✅ Systemd services created"

# ===================================================================
# BƯỚC 9: CẤU HÌNH FIREWALL
# ===================================================================
echo "🔥 Configuring firewall..."

# Enable UFW và cấu hình rules
ufw --force enable
ufw default deny incoming
ufw default allow outgoing
ufw allow ssh
ufw allow 'Nginx Full'

echo "✅ Firewall configured"

# ===================================================================
# BƯỚC 10: SET PERMISSIONS
# ===================================================================
echo "🔐 Setting permissions..."

# Set ownership
chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR
chmod -R 755 $INSTALL_DIR
chmod -R 644 $INSTALL_DIR/certificates/*
chmod 600 $INSTALL_DIR/certificates/server.key

# Database permissions
chmod 664 $INSTALL_DIR/data/BioWeb.db 2>/dev/null || true
chown $SERVICE_USER:$SERVICE_USER $INSTALL_DIR/data/BioWeb.db 2>/dev/null || true

echo "✅ Permissions set"

# ===================================================================
# BƯỚC 11: START SERVICES
# ===================================================================
echo "🚀 Starting services..."

# Start BioWeb server
systemctl start bioweb-server
systemctl status bioweb-server --no-pager

# Restart Nginx
systemctl restart nginx
systemctl status nginx --no-pager

echo "✅ Services started"

# ===================================================================
# BƯỚC 12: CẤU HÌNH LET'S ENCRYPT (TÙY CHỌN)
# ===================================================================
echo "🔒 Setting up Let's Encrypt SSL..."
echo "⚠️  QUAN TRỌNG: Đảm bảo domain $DOMAIN đã trỏ về server này!"
read -p "Bạn có muốn cài đặt Let's Encrypt SSL certificate? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    certbot --nginx -d $DOMAIN -d www.$DOMAIN --email $EMAIL --agree-tos --non-interactive
    echo "✅ Let's Encrypt SSL configured"
else
    echo "⏭️  Bỏ qua Let's Encrypt. Bạn có thể chạy sau: certbot --nginx -d $DOMAIN"
fi

# ===================================================================
# BƯỚC 13: KIỂM TRA VÀ HOÀN TẤT
# ===================================================================
echo ""
echo "====================================================================="
echo "🎉 DEPLOYMENT COMPLETED!"
echo "====================================================================="
echo ""
echo "📋 THÔNG TIN DEPLOYMENT:"
echo "   • Domain: https://$DOMAIN"
echo "   • API: https://$DOMAIN/api"
echo "   • Install Directory: $INSTALL_DIR"
echo "   • Database: $INSTALL_DIR/data/BioWeb.db"
echo "   • Logs: journalctl -u bioweb-server -f"
echo ""
echo "� ADMIN LOGIN:"
echo "   • Username: $ADMIN_USERNAME"
echo "   • Password: $ADMIN_PASSWORD"
echo "   • Admin URL: https://$DOMAIN/admin"
echo ""
echo "�🔧 LỆNH QUẢN LÝ HỮU ÍCH:"
echo "   • Restart server: systemctl restart bioweb-server"
echo "   • View logs: journalctl -u bioweb-server -f"
echo "   • Nginx reload: systemctl reload nginx"
echo "   • Database backup: cp $INSTALL_DIR/data/BioWeb.db $INSTALL_DIR/backups/"
echo ""
echo "⚠️  QUAN TRỌNG - CẦN LÀM THÊM:"
echo "   1. Kiểm tra domain $DOMAIN đã trỏ về server này"
echo "   2. Chạy Let's Encrypt nếu chưa: certbot --nginx -d $DOMAIN"
echo "   3. Đổi JWT secret key trong production"
echo "   4. Backup database định kỳ"
echo "   5. Cấu hình monitoring và logging"
echo ""
echo "🌐 Truy cập website: https://$DOMAIN"
echo "====================================================================="

# Test API endpoint
echo "🧪 Testing API endpoint..."
sleep 5
if curl -k -s https://$DOMAIN/api/SiteConfiguration/about > /dev/null; then
    echo "✅ API endpoint responding"
else
    echo "❌ API endpoint not responding - check logs: journalctl -u bioweb-server -f"
fi

echo ""
echo "🚀 Deployment script completed!"
echo "Check the website at: https://$DOMAIN"
