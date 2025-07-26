# BioWeb Production Deployment Guide

## 🚀 Hướng dẫn Deploy Production

### Yêu cầu hệ thống
- Ubuntu 20.04+ hoặc Debian 11+
- RAM: tối thiểu 1GB, khuyến nghị 2GB+
- Disk: tối thiểu 10GB free space
- Domain đã trỏ về server IP

### Bước 1: Chuẩn bị server

```bash
# Update server
sudo apt update && sudo apt upgrade -y

# Clone repository
git clone https://github.com/your-username/your-repo.git
cd your-repo
```

### Bước 2: Cấu hình script deployment

**⚠️ QUAN TRỌNG: Sửa các biến sau trong file `deploy-production.sh`:**

```bash
# Mở file để sửa
nano deploy-production.sh

# Sửa các biến này:
DOMAIN="yourdomain.com"                    # ➡️ Sửa thành domain của bạn
EMAIL="admin@yourdomain.com"               # ➡️ Sửa thành email của bạn
GIT_REPO="https://github.com/user/repo.git" # ➡️ Sửa thành repo của bạn
DB_PASSWORD="your_secure_password_here"    # ➡️ Tạo password mạnh
JWT_SECRET="your_super_secret_jwt_key_here" # ➡️ Tạo JWT secret key mạnh
SUDO_PASSWORD="1234"                       # ➡️ Sudo password của server
ADMIN_USERNAME="admin"                     # ➡️ Admin username cho website
ADMIN_PASSWORD="123"                       # ➡️ Admin password cho website
```

### Bước 3: Chạy deployment

```bash
# Chạy script với quyền root
sudo ./deploy-production.sh
```

Script sẽ tự động:
- ✅ Cài đặt .NET 9.0, EF Core Tools, Nginx, SQLite
- ✅ Tạo user và thư mục hệ thống
- ✅ Build project (Server + Client)
- ✅ Cấu hình database SQLite với migration
- ✅ Cập nhật admin username/password trong database
- ✅ Tạo SSL certificate tạm thời
- ✅ Cấu hình Nginx reverse proxy
- ✅ Tạo systemd service
- ✅ Cấu hình firewall
- ✅ Start tất cả services
- ✅ Sử dụng sudo password tự động

### Bước 4: Cấu hình Let's Encrypt (Khuyến nghị)

```bash
# Sau khi deployment xong, cài SSL certificate thật
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

## 🔧 Quản lý sau deployment

### Lệnh hữu ích

```bash
# Xem logs server
sudo journalctl -u bioweb-server -f

# Restart server
sudo systemctl restart bioweb-server

# Restart Nginx
sudo systemctl restart nginx

# Backup database
sudo cp /var/www/bioweb/data/BioWeb.db /var/www/bioweb/backups/backup-$(date +%Y%m%d).db

# Update code
cd /var/www/bioweb/source
sudo git pull origin main
sudo dotnet publish BioWeb.server/BioWeb.server.csproj -c Release -o /var/www/bioweb/server
sudo dotnet publish BioWeb.client/BioWeb.client.csproj -c Release -o /var/www/bioweb/client
sudo systemctl restart bioweb-server
sudo systemctl reload nginx
```

### Cấu trúc thư mục

```
/var/www/bioweb/
├── server/          # Server API files
├── client/          # Client static files  
├── data/            # Database SQLite
├── certificates/    # SSL certificates
├── logs/            # Application logs
├── backups/         # Database backups
└── source/          # Source code
```

## 🛡️ Bảo mật

### Những việc cần làm sau deployment:

1. **Đổi JWT Secret Key** trong production
2. **Backup database** định kỳ
3. **Cập nhật hệ thống** thường xuyên
4. **Monitor logs** để phát hiện lỗi
5. **Cấu hình fail2ban** để chống brute force

### Backup tự động

Tạo cron job backup hàng ngày:

```bash
# Mở crontab
sudo crontab -e

# Thêm dòng này (backup lúc 2h sáng hàng ngày)
0 2 * * * cp /var/www/bioweb/data/BioWeb.db /var/www/bioweb/backups/auto-backup-$(date +\%Y\%m\%d).db
```

## 🐛 Troubleshooting

### Lỗi thường gặp:

1. **Service không start**: Kiểm tra logs `journalctl -u bioweb-server`
2. **Database permission**: `sudo chown bioweb:bioweb /var/www/bioweb/data/BioWeb.db`
3. **Nginx 502**: Kiểm tra server API có chạy không
4. **SSL certificate**: Chạy lại `certbot --nginx`

### Kiểm tra health:

```bash
# Test API
curl https://yourdomain.com/api/SiteConfiguration/about

# Test website
curl https://yourdomain.com

# Check services
sudo systemctl status bioweb-server
sudo systemctl status nginx
```

## 📞 Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. Logs: `journalctl -u bioweb-server -f`
2. Nginx logs: `/var/log/nginx/error.log`
3. Database permissions
4. Firewall settings: `ufw status`
