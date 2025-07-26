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

# Install essential packages và dependencies đầy đủ
apt install -y curl wget git nginx sqlite3 ufw certbot python3-certbot-nginx \
    software-properties-common apt-transport-https ca-certificates \
    gnupg lsb-release build-essential unzip zip htop nano vim \
    openssl rsync net-tools iptables-persistent fail2ban \
    python3 python3-pip nodejs npm

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

# Cài đặt thêm các công cụ hữu ích
echo "📦 Installing additional useful tools..."

# Cài đặt Docker (nếu cần)
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    usermod -aG docker root
    rm get-docker.sh
fi

# Cài đặt PM2 cho Node.js process management
npm install -g pm2

# Cài đặt các Python packages hữu ích
pip3 install --upgrade pip
pip3 install requests beautifulsoup4 lxml

# Cấu hình timezone
timedatectl set-timezone Asia/Ho_Chi_Minh

# Tăng file limits
echo "* soft nofile 65536" >> /etc/security/limits.conf
echo "* hard nofile 65536" >> /etc/security/limits.conf
echo "root soft nofile 65536" >> /etc/security/limits.conf
echo "root hard nofile 65536" >> /etc/security/limits.conf

# Cấu hình kernel parameters
echo "net.core.somaxconn = 65536" >> /etc/sysctl.conf
echo "net.ipv4.tcp_max_syn_backlog = 65536" >> /etc/sysctl.conf
echo "net.core.netdev_max_backlog = 5000" >> /etc/sysctl.conf
sysctl -p

echo "✅ All dependencies and tools installed successfully"

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

    # SSL Security Settings
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-RSA-AES128-SHA256:ECDHE-RSA-AES256-SHA384;
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # Security headers
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    add_header X-XSS-Protection "1; mode=block";
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin";
    add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self';";

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

    # API proxy to backend với CORS support
    location /api/ {
        # CORS headers
        add_header 'Access-Control-Allow-Origin' '*' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, OPTIONS' always;
        add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization' always;
        add_header 'Access-Control-Expose-Headers' 'Content-Length,Content-Range' always;

        # Handle preflight requests
        if (\$request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*';
            add_header 'Access-Control-Allow-Methods' 'GET, POST, PUT, DELETE, OPTIONS';
            add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range,Authorization';
            add_header 'Access-Control-Max-Age' 1728000;
            add_header 'Content-Type' 'text/plain; charset=utf-8';
            add_header 'Content-Length' 0;
            return 204;
        }

        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_buffering off;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
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
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=ProductionDomain=$DOMAIN
Environment=ConnectionStrings__DefaultConnection="Data Source=$INSTALL_DIR/data/BioWeb.db"
Environment=ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd và enable services
systemctl daemon-reload
systemctl enable bioweb-server

echo "✅ Systemd services created"

# ===================================================================
# BƯỚC 9: CẤU HÌNH FIREWALL VÀ MỞ TẤT CẢ PORT CẦN THIẾT
# ===================================================================
echo "🔥 Configuring firewall and opening all necessary ports..."

# Reset UFW về mặc định
ufw --force reset

# Cấu hình UFW rules chi tiết
ufw default deny incoming
ufw default allow outgoing

# Mở các port cơ bản
ufw allow ssh                    # Port 22 - SSH
ufw allow 80/tcp                 # Port 80 - HTTP
ufw allow 443/tcp                # Port 443 - HTTPS
ufw allow 'Nginx Full'           # Nginx HTTP + HTTPS

# Mở thêm các port phổ biến cho development
ufw allow 3000/tcp               # React dev server
ufw allow 5000/tcp               # ASP.NET Core default
ufw allow 5001/tcp               # ASP.NET Core HTTPS
ufw allow 8080/tcp               # Alternative HTTP
ufw allow 8443/tcp               # Alternative HTTPS

# Mở port cho database (nếu cần remote access)
ufw allow 1433/tcp               # SQL Server
ufw allow 3306/tcp               # MySQL
ufw allow 5432/tcp               # PostgreSQL

# Mở port cho monitoring tools
ufw allow 9090/tcp               # Prometheus
ufw allow 3001/tcp               # Grafana

# Enable UFW
ufw --force enable

# Hiển thị status
ufw status verbose

echo "✅ Firewall configured with all necessary ports opened"

# Cấu hình thêm iptables để đảm bảo traffic flow
echo "🌐 Configuring additional network settings..."

# Đảm bảo iptables cho phép traffic
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
iptables -A INPUT -p tcp --dport 443 -j ACCEPT
iptables -A INPUT -p tcp --dport 22 -j ACCEPT
iptables -A INPUT -p tcp --dport 5000 -j ACCEPT

# Save iptables rules
iptables-save > /etc/iptables/rules.v4

# Cấu hình sysctl cho network performance
echo "net.ipv4.ip_forward = 1" >> /etc/sysctl.conf
echo "net.ipv4.conf.all.accept_redirects = 0" >> /etc/sysctl.conf
echo "net.ipv4.conf.all.send_redirects = 0" >> /etc/sysctl.conf
echo "net.ipv4.conf.all.accept_source_route = 0" >> /etc/sysctl.conf
sysctl -p

echo "✅ Network configuration completed"

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
