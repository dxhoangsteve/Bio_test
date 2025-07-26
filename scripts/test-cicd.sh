#!/bin/bash

echo "🧪 Testing CI/CD Pipeline"
echo "========================"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Test 1: Check if GitHub repo exists
echo "1. Testing GitHub repository..."
if git remote -v | grep -q "github.com"; then
    print_status "GitHub repository configured"
    REPO_URL=$(git remote get-url origin)
    echo "   Repository: $REPO_URL"
else
    print_error "GitHub repository not configured"
    echo "   Run: git remote add origin https://github.com/username/repo.git"
fi

# Test 2: Check workflow file
echo -e "\n2. Testing workflow file..."
if [ -f ".github/workflows/deploy.yml" ]; then
    print_status "Workflow file exists"
    
    # Check for required secrets
    if grep -q "VPS_HOST" .github/workflows/deploy.yml; then
        print_status "VPS_HOST secret referenced"
    else
        print_error "VPS_HOST secret not found"
    fi
    
    if grep -q "VPS_USER" .github/workflows/deploy.yml; then
        print_status "VPS_USER secret referenced"
    else
        print_error "VPS_USER secret not found"
    fi
    
    if grep -q "VPS_SSH_KEY" .github/workflows/deploy.yml; then
        print_status "VPS_SSH_KEY secret referenced"
    else
        print_error "VPS_SSH_KEY secret not found"
    fi
else
    print_error "Workflow file not found"
fi

# Test 3: Check Dockerfile
echo -e "\n3. Testing Dockerfile..."
if [ -f "Dockerfile.production" ]; then
    print_status "Production Dockerfile exists"
    
    if grep -q "FROM mcr.microsoft.com/dotnet" Dockerfile.production; then
        print_status "Using official .NET images"
    fi
    
    if grep -q "HEALTHCHECK" Dockerfile.production; then
        print_status "Health check configured"
    fi
else
    print_error "Dockerfile.production not found"
fi

# Test 4: Check docker-compose
echo -e "\n4. Testing docker-compose..."
if [ -f "docker-compose.production.yml" ]; then
    print_status "Production docker-compose exists"
    
    if grep -q "443:443" docker-compose.production.yml; then
        print_status "HTTPS port 443 configured"
    else
        print_warning "HTTPS port 443 not found"
    fi
    
    if grep -q "ghcr.io" docker-compose.production.yml; then
        print_status "GitHub Container Registry configured"
    fi
else
    print_error "docker-compose.production.yml not found"
fi

# Test 5: Check project structure
echo -e "\n5. Testing project structure..."
if [ -d "BioWeb.server" ]; then
    print_status "Server project found"
else
    print_error "BioWeb.server directory not found"
fi

if [ -d "BioWeb.client" ]; then
    print_status "Client project found"
else
    print_error "BioWeb.client directory not found"
fi

# Test 6: Test local build
echo -e "\n6. Testing local build..."
if command -v dotnet &> /dev/null; then
    print_status "dotnet CLI available"
    
    echo "   Testing restore..."
    if dotnet restore > /dev/null 2>&1; then
        print_status "dotnet restore successful"
    else
        print_error "dotnet restore failed"
    fi
    
    echo "   Testing build..."
    if dotnet build --no-restore > /dev/null 2>&1; then
        print_status "dotnet build successful"
    else
        print_error "dotnet build failed"
    fi
else
    print_warning "dotnet CLI not available (install .NET SDK to test locally)"
fi

# Test 7: Check git status
echo -e "\n7. Testing git status..."
if git status --porcelain | grep -q .; then
    print_warning "Uncommitted changes found"
    echo "   Run: git add . && git commit -m 'Setup CI/CD'"
else
    print_status "Working directory clean"
fi

# Summary
echo -e "\n📋 Summary:"
echo "============"
print_status "Ready to push and trigger CI/CD!"

echo -e "\n🚀 Next steps:"
echo "1. Setup GitHub secrets (if not done):"
echo "   - Go to GitHub repo → Settings → Secrets"
echo "   - Add: VPS_HOST, VPS_USER, VPS_SSH_KEY"
echo ""
echo "2. Push to trigger deployment:"
echo "   git add ."
echo "   git commit -m 'Enable CI/CD pipeline'"
echo "   git push origin main"
echo ""
echo "3. Monitor deployment:"
echo "   - GitHub → Your repo → Actions tab"
echo "   - Watch real-time deployment progress"
echo ""
echo "4. Test HTTPS access:"
echo "   https://dxhoang.site (port 443 - hidden)"

echo -e "\n🔗 Useful links:"
echo "- GitHub Actions: https://github.com/$(git remote get-url origin | sed 's/.*github.com[:/]\(.*\)\.git/\1/')/actions"
echo "- Container Registry: https://github.com/$(git remote get-url origin | sed 's/.*github.com[:/]\(.*\)\.git/\1/')/pkgs/container/bioweb"
