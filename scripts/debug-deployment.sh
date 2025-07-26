#!/bin/bash

echo "🔍 Debugging deployment on VPS..."

# Check if running on VPS
if [ ! -f "/root/bioweb-production/docker-compose.production.yml" ]; then
    echo "❌ Not on VPS or docker-compose file missing"
    exit 1
fi

cd /root/bioweb-production

echo "📊 Container status:"
docker-compose -f docker-compose.production.yml ps

echo ""
echo "📋 App container logs (last 50 lines):"
docker logs bioweb-app --tail 50

echo ""
echo "📋 Nginx container logs (last 20 lines):"
docker logs bioweb-nginx --tail 20

echo ""
echo "🔍 Testing app directly:"
echo "Port 5000 (app):"
curl -f http://localhost:5000/health 2>/dev/null && echo "✅ App responding" || echo "❌ App not responding"

echo ""
echo "Port 80 (nginx):"
curl -f http://localhost:80/ 2>/dev/null && echo "✅ Nginx responding" || echo "❌ Nginx not responding"

echo ""
echo "🔍 Network info:"
docker network ls | grep bioweb

echo ""
echo "🔍 Image info:"
docker images | grep bioweb

echo ""
echo "🔍 Process info:"
docker exec bioweb-app ps aux 2>/dev/null || echo "❌ Cannot access app container"

echo ""
echo "🔍 Environment check:"
docker exec bioweb-app env | grep ASPNETCORE 2>/dev/null || echo "❌ Cannot check environment"

echo ""
echo "🔍 File system check:"
docker exec bioweb-app ls -la /app/ 2>/dev/null || echo "❌ Cannot check app files"

echo ""
echo "🔍 Database check:"
docker exec bioweb-app ls -la /app/data/ 2>/dev/null || echo "❌ Cannot check data directory"
