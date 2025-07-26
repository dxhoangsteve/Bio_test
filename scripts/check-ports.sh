#!/bin/bash

echo "🔍 Checking ports and network configuration..."

# Check if ports are open
echo "📡 Port status:"
netstat -tlnp | grep -E ':80|:443' || echo "No services on ports 80/443"

echo ""
echo "🔥 Firewall status:"
ufw status

echo ""
echo "🐳 Docker containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "🌐 Network connectivity:"
echo "Testing localhost:80..."
curl -s -o /dev/null -w "HTTP %{http_code} - %{time_total}s\n" http://localhost:80/ || echo "❌ Port 80 not responding"

echo "Testing localhost:443..."
curl -k -s -o /dev/null -w "HTTP %{http_code} - %{time_total}s\n" https://localhost:443/ || echo "❌ Port 443 not responding"

echo ""
echo "🔍 External connectivity:"
echo "Testing dxhoang.site:80..."
curl -s -o /dev/null -w "HTTP %{http_code} - %{time_total}s\n" http://dxhoang.site/ || echo "❌ External port 80 not responding"

echo "Testing dxhoang.site:443..."
curl -k -s -o /dev/null -w "HTTP %{http_code} - %{time_total}s\n" https://dxhoang.site/ || echo "❌ External port 443 not responding"

echo ""
echo "📋 Container logs (last 10 lines):"
echo "--- Nginx logs ---"
docker logs bioweb-nginx --tail 10 2>/dev/null || echo "Cannot get nginx logs"

echo ""
echo "--- App logs ---"
docker logs bioweb-app --tail 10 2>/dev/null || echo "Cannot get app logs"

echo ""
echo "🔧 Troubleshooting commands:"
echo "1. Check firewall: ufw status"
echo "2. Check containers: docker ps"
echo "3. Check nginx config: docker exec bioweb-nginx nginx -t"
echo "4. Restart containers: docker-compose -f docker-compose.production.yml restart"
echo "5. View full logs: docker logs bioweb-nginx && docker logs bioweb-app"
