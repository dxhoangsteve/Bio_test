#!/bin/bash

# Test deployment script
VPS_HOST="dxhoang.site"
ADMIN_USER="admin"
ADMIN_PASS="BioWeb2024!"

echo "🧪 Testing BioWeb deployment on $VPS_HOST"
echo "================================================"

# Test 1: HTTP to HTTPS redirect
echo "1️⃣ Testing HTTP to HTTPS redirect..."
HTTP_RESPONSE=$(curl -s -I http://$VPS_HOST/ | head -n 1)
if echo "$HTTP_RESPONSE" | grep -q "301\|302"; then
    echo "✅ HTTP redirect working"
else
    echo "❌ HTTP redirect not working: $HTTP_RESPONSE"
fi

# Test 2: HTTPS homepage
echo ""
echo "2️⃣ Testing HTTPS homepage..."
HTTPS_RESPONSE=$(curl -k -s -w "%{http_code}" https://$VPS_HOST/ -o /dev/null)
if [ "$HTTPS_RESPONSE" = "200" ]; then
    echo "✅ HTTPS homepage working (200)"
else
    echo "❌ HTTPS homepage failed: $HTTPS_RESPONSE"
fi

# Test 3: API health check
echo ""
echo "3️⃣ Testing API health..."
HEALTH_RESPONSE=$(curl -k -s https://$VPS_HOST/api/health)
if [ "$HEALTH_RESPONSE" = "OK" ]; then
    echo "✅ API health check working"
else
    echo "❌ API health check failed: $HEALTH_RESPONSE"
fi

# Test 4: Admin login page
echo ""
echo "4️⃣ Testing admin login page..."
LOGIN_RESPONSE=$(curl -k -s -w "%{http_code}" https://$VPS_HOST/admin/login -o /dev/null)
if [ "$LOGIN_RESPONSE" = "200" ]; then
    echo "✅ Admin login page accessible"
else
    echo "❌ Admin login page failed: $LOGIN_RESPONSE"
fi

# Test 5: Admin API login
echo ""
echo "5️⃣ Testing admin API login..."
LOGIN_API_RESPONSE=$(curl -k -s -X POST https://$VPS_HOST/api/auth/admin/login \
  -H "Content-Type: application/json" \
  -d "{\"Username\":\"$ADMIN_USER\",\"Password\":\"$ADMIN_PASS\"}")

if echo "$LOGIN_API_RESPONSE" | grep -q '"Success":true'; then
    echo "✅ Admin API login working"
    # Extract token for further tests
    TOKEN=$(echo "$LOGIN_API_RESPONSE" | grep -o '"Token":"[^"]*"' | cut -d'"' -f4)
    echo "🔑 Token extracted: ${TOKEN:0:20}..."
else
    echo "❌ Admin API login failed: $LOGIN_API_RESPONSE"
    TOKEN=""
fi

# Test 6: Protected API endpoint (if token available)
if [ -n "$TOKEN" ]; then
    echo ""
    echo "6️⃣ Testing protected API endpoint..."
    PROTECTED_RESPONSE=$(curl -k -s -w "%{http_code}" \
      -H "Authorization: Bearer $TOKEN" \
      https://$VPS_HOST/api/siteconfiguration -o /dev/null)
    
    if [ "$PROTECTED_RESPONSE" = "200" ]; then
        echo "✅ Protected API working"
    else
        echo "❌ Protected API failed: $PROTECTED_RESPONSE"
    fi
fi

# Test 7: Static files (CSS/JS)
echo ""
echo "7️⃣ Testing static files..."
CSS_RESPONSE=$(curl -k -s -w "%{http_code}" https://$VPS_HOST/_framework/blazor.web.js -o /dev/null)
if [ "$CSS_RESPONSE" = "200" ]; then
    echo "✅ Static files working"
else
    echo "❌ Static files failed: $CSS_RESPONSE"
fi

# Test 8: Database connectivity (via API)
echo ""
echo "8️⃣ Testing database connectivity..."
DB_RESPONSE=$(curl -k -s https://$VPS_HOST/api/siteconfiguration/public)
if echo "$DB_RESPONSE" | grep -q '"Success":true'; then
    echo "✅ Database connectivity working"
else
    echo "❌ Database connectivity failed: $DB_RESPONSE"
fi

echo ""
echo "================================================"
echo "🎯 Test Summary:"
echo "- Website: https://$VPS_HOST"
echo "- Admin: https://$VPS_HOST/admin/login"
echo "- Credentials: $ADMIN_USER / $ADMIN_PASS"
echo ""
echo "📋 Manual tests to perform:"
echo "1. Browse to website and check all pages"
echo "2. Login to admin and test all features"
echo "3. Upload files (avatar, project images)"
echo "4. Create/edit content (projects, blog posts)"
echo "5. Check responsive design on mobile"
echo ""
echo "✅ Automated testing completed!"
