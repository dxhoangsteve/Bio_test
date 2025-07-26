# 🚀 Enable CI/CD for BioWeb

## Quick Start Guide

### 1. 📋 Pre-check
```bash
# Test if everything is ready
chmod +x scripts/test-cicd.sh
./scripts/test-cicd.sh
```

### 2. 🔧 Setup GitHub Repository

#### A. Create GitHub Repository
1. Go to [GitHub](https://github.com)
2. Click "New repository"
3. Name: `bioweb` (or your preferred name)
4. Make it **Public** (for free GitHub Actions)
5. Don't initialize with README (we have code already)

#### B. Connect Local Repository
```bash
# Add GitHub remote
git remote add origin https://github.com/YOUR_USERNAME/bioweb.git

# Verify
git remote -v
```

### 3. 🔐 Setup GitHub Secrets

Go to: **GitHub Repository → Settings → Secrets and variables → Actions**

Click **"New repository secret"** and add these 3 secrets:

| Secret Name | Value | How to get |
|-------------|-------|------------|
| `VPS_HOST` | Your VPS IP address | `curl ifconfig.me` on VPS |
| `VPS_USER` | `root` | SSH username |
| `VPS_SSH_KEY` | Private SSH key | See below ⬇️ |

#### Generate SSH Key (if needed):
```bash
# On your local machine
ssh-keygen -t rsa -b 4096 -f ~/.ssh/github_actions -N ""

# Copy public key to VPS
ssh-copy-id -i ~/.ssh/github_actions.pub root@YOUR_VPS_IP

# Get private key for GitHub secret
cat ~/.ssh/github_actions
```

Copy the **entire private key** (including `-----BEGIN` and `-----END` lines) to `VPS_SSH_KEY` secret.

### 4. 🏗️ Setup VPS

Run this **once** on your VPS:
```bash
# Download and run VPS setup
curl -fsSL https://raw.githubusercontent.com/YOUR_USERNAME/bioweb/main/scripts/setup-vps.sh -o setup-vps.sh
chmod +x setup-vps.sh
sudo ./setup-vps.sh
```

### 5. 🚀 Enable CI/CD

#### A. Update GitHub username in workflow
Edit `.github/workflows/deploy.yml` and replace `yourusername` with your actual GitHub username.

#### B. Push to GitHub
```bash
# Add all files
git add .

# Commit
git commit -m "🚀 Enable CI/CD pipeline with HTTPS support"

# Push to main branch (this triggers CI/CD!)
git push origin main
```

### 6. 📊 Monitor Deployment

1. **Go to GitHub Actions**:
   - GitHub → Your Repository → **Actions** tab
   - You'll see the workflow running

2. **Watch Progress**:
   - Click on the running workflow
   - See real-time logs
   - Green ✅ = Success, Red ❌ = Failed

3. **Check Deployment**:
   ```bash
   # Test HTTPS (port 443 - hidden)
   curl -k https://dxhoang.site
   
   # Should redirect HTTP to HTTPS
   curl -I http://dxhoang.site
   ```

## 🎯 What Happens When You Push?

1. **GitHub Actions Triggers** 📡
2. **Runs Tests** 🧪 (dotnet test)
3. **Builds Docker Image** 🐳
4. **Pushes to Registry** 📦 (ghcr.io)
5. **Deploys to VPS** 🚀
6. **Runs Health Checks** ❤️
7. **Reports Success** ✅

## 🌐 Access Your App

After successful deployment:

- **HTTPS**: https://dxhoang.site (port 443 - hidden)
- **HTTP**: http://dxhoang.site (redirects to HTTPS)

## 🔄 Development Workflow

```bash
# Make changes to your code
nano BioWeb.server/Controllers/HomeController.cs

# Commit and push
git add .
git commit -m "Update home page"
git push origin main

# 🎉 Automatic deployment starts!
```

## 🛠️ Troubleshooting

### ❌ SSH Connection Failed
```bash
# Test SSH manually
ssh -i ~/.ssh/github_actions root@YOUR_VPS_IP

# Check VPS firewall
sudo ufw status
sudo ufw allow 22
```

### ❌ Docker Build Failed
- Check `.github/workflows/deploy.yml` syntax
- Verify all project files are committed
- Check build logs in GitHub Actions

### ❌ Deployment Failed
```bash
# On VPS, check logs
cd /root/bioweb-production
docker-compose -f docker-compose.production.yml logs -f
```

### ❌ HTTPS Not Working
```bash
# Check nginx
docker-compose -f docker-compose.production.yml logs nginx

# Check SSL certificate
openssl x509 -in ssl/server.crt -text -noout
```

## 📈 Benefits You Get

✅ **Auto Deployment**: Push code → Live in minutes
✅ **HTTPS Ready**: Secure with port 443 (hidden)
✅ **Professional**: Industry standard CI/CD
✅ **Version Control**: Every change tracked
✅ **Easy Rollback**: Revert to any previous version
✅ **Team Ready**: Multiple developers can contribute
✅ **Monitoring**: Real-time deployment status

## 🎉 Success Indicators

When everything works:

1. ✅ GitHub Actions shows green checkmark
2. ✅ `https://dxhoang.site` loads your app
3. ✅ `http://dxhoang.site` redirects to HTTPS
4. ✅ No port number needed in URL
5. ✅ SSL certificate works (no browser warnings)

## 🆘 Need Help?

1. **Check GitHub Actions logs** first
2. **Run test script**: `./scripts/test-cicd.sh`
3. **Check VPS logs**: `docker-compose logs -f`
4. **Verify secrets** are set correctly in GitHub

---

**Ready? Run the test script and then push to GitHub! 🚀**
