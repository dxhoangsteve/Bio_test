#!/bin/bash

echo "🚀 BioWeb VPS Setup for CI/CD"
echo "=============================="

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    print_error "Please run as root"
    exit 1
fi

# Update system
print_status "Updating system..."
apt update && apt upgrade -y

# Install Docker
print_status "Installing Docker..."
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
rm get-docker.sh

# Install Docker Compose
print_status "Installing Docker Compose..."
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
ln -sf /usr/local/bin/docker-compose /usr/bin/docker-compose

# Start Docker
systemctl start docker
systemctl enable docker

# Create deployment directory
print_status "Creating deployment directory..."
mkdir -p /root/bioweb-production
cd /root/bioweb-production

# Create required directories
mkdir -p data uploads ssl logs

# Generate SSL certificate
print_status "Generating SSL certificate..."
openssl req -x509 -newkey rsa:2048 -keyout ssl/server.key -out ssl/server.crt -days 365 -nodes \
    -subj "/C=VN/ST=HCM/L=HCM/O=BioWeb/CN=dxhoang.site"
chmod 600 ssl/server.key
chmod 644 ssl/server.crt

# Create nginx config
print_status "Creating nginx configuration..."
cat > nginx.conf << 'EOF'
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;
    
    # Logging
    access_log /var/log/nginx/access.log;
    error_log /var/log/nginx/error.log;
    
    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/xml+rss application/json;

    upstream bioweb {
        server bioweb:80;
    }

    # HTTP server (redirect to HTTPS)
    server {
        listen 80;
        server_name dxhoang.site www.dxhoang.site _;
        return 301 https://$host$request_uri;
    }

    # HTTPS server
    server {
        listen 443 ssl http2;
        server_name dxhoang.site www.dxhoang.site _;

        # SSL configuration
        ssl_certificate /etc/nginx/ssl/server.crt;
        ssl_certificate_key /etc/nginx/ssl/server.key;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384;

        # Security headers
        add_header X-Frame-Options DENY;
        add_header X-Content-Type-Options nosniff;
        add_header X-XSS-Protection "1; mode=block";
        add_header Strict-Transport-Security "max-age=31536000; includeSubDomains";

        # Main application
        location / {
            proxy_pass http://bioweb;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_buffering off;
        }

        # API endpoints
        location /api/ {
            proxy_pass http://bioweb/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Static files
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            proxy_pass http://bioweb;
            proxy_set_header Host $host;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }

        # Upload files
        location /uploads/ {
            alias /var/www/uploads/;
            expires 1d;
            add_header Cache-Control "public";
        }
    }
}
EOF

# Set permissions
chmod 755 data uploads
chmod 644 nginx.conf

# Create environment file
cat > .env << 'EOF'
GITHUB_REPOSITORY_OWNER=yourusername
ASPNETCORE_ENVIRONMENT=Production
EOF

print_status "VPS setup completed! 🎉"
print_warning "Next steps:"
echo "1. Update .env file with your GitHub username"
echo "2. Setup GitHub secrets (VPS_HOST, VPS_USER, VPS_SSH_KEY)"
echo "3. Push your code to GitHub"
echo "4. GitHub Actions will automatically deploy!"

print_status "Current directory: $(pwd)"
print_status "Files created:"
ls -la
