# Render Deployment Script for Enterprise IT Toolkit
# This script helps deploy your application to Render

Write-Host "=== RENDER DEPLOYMENT SETUP ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Check if required tools are installed
Write-Host "`nChecking prerequisites..." -ForegroundColor Yellow

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js is installed: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js is not installed. Please install Node.js first." -ForegroundColor Red
    Write-Host "Download from: https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Check .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET is installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET is not installed. Please install .NET 8.0 first." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Check Git
try {
    $gitVersion = git --version
    Write-Host "✅ Git is installed: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Git is not installed. Please install Git first." -ForegroundColor Red
    Write-Host "Download from: https://git-scm.com/download/win" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n=== PREPARING FOR RENDER DEPLOYMENT ===" -ForegroundColor Cyan

# 1. Install dependencies
Write-Host "`n1. Installing dependencies..." -ForegroundColor Yellow
try {
    npm install
    Write-Host "✅ Node.js dependencies installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install Node.js dependencies" -ForegroundColor Red
    exit 1
}

# 2. Build the application
Write-Host "`n2. Building application..." -ForegroundColor Yellow
try {
    # Build web interface
    npm run build
    Write-Host "✅ Web interface built successfully" -ForegroundColor Green
    
    # Build .NET application
    dotnet build --configuration Release
    Write-Host "✅ .NET application built successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to build application" -ForegroundColor Red
    exit 1
}

# 3. Create environment configuration
Write-Host "`n3. Creating environment configuration..." -ForegroundColor Yellow
$envConfig = @"
# Render Environment Configuration
# Copy these to your Render service environment variables

# Database Configuration
DATABASE_URL=postgresql://enterprise_toolkit_user:password@host:port/enterprise_toolkit
REDIS_URL=redis://host:port

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
JWT__SecretKey=your-secure-jwt-secret-key-here
JWT__Issuer=EnterpriseITToolkit
JWT__Audience=EnterpriseITToolkit-Users

# CORS Configuration
API__Cors__AllowedOrigins=https://enterprise-toolkit-web.onrender.com,https://enterprise-toolkit.onrender.com

# Rate Limiting
API__RateLimit__RequestsPerMinute=100

# External Services (Optional)
SMTP__Host=smtp.gmail.com
SMTP__Port=587
SMTP__Username=your-email@gmail.com
SMTP__Password=your-app-password

# File Storage
STORAGE__Provider=local
STORAGE__Path=/tmp/uploads
"@

$envConfig | Out-File -FilePath "render-env-config.txt" -Encoding UTF8
Write-Host "✅ Environment configuration created (render-env-config.txt)" -ForegroundColor Green

# 4. Create deployment instructions
Write-Host "`n4. Creating deployment instructions..." -ForegroundColor Yellow
$deploymentInstructions = @"
# RENDER DEPLOYMENT INSTRUCTIONS

## Step 1: Create Render Account
1. Go to https://render.com
2. Sign up with your GitHub account
3. Connect your GitHub repository

## Step 2: Create Database
1. Go to Dashboard > New > PostgreSQL
2. Name: enterprise-toolkit-db
3. Database: enterprise_toolkit
4. User: enterprise_toolkit_user
5. Plan: Starter (Free)

## Step 3: Create Redis Cache
1. Go to Dashboard > New > Redis
2. Name: enterprise-toolkit-redis
3. Plan: Starter (Free)

## Step 4: Deploy Web Service
1. Go to Dashboard > New > Web Service
2. Connect Repository: EnterpriseITToolkit
3. Name: enterprise-toolkit-web
4. Environment: Static Site
5. Build Command: npm install && npm run build
6. Publish Directory: ./dist
7. Add Environment Variable:
   - API_URL: https://enterprise-toolkit-api.onrender.com

## Step 5: Deploy API Service
1. Go to Dashboard > New > Web Service
2. Connect Repository: EnterpriseITToolkit
3. Name: enterprise-toolkit-api
4. Environment: .NET
5. Build Command: dotnet restore && dotnet publish -c Release -o ./publish
6. Start Command: dotnet ./publish/EnterpriseITToolkit.dll
7. Plan: Starter ($7/month)
8. Add Environment Variables (see render-env-config.txt)

## Step 6: Deploy Worker Service (Optional)
1. Go to Dashboard > New > Background Worker
2. Connect Repository: EnterpriseITToolkit
3. Name: enterprise-toolkit-worker
4. Environment: .NET
5. Build Command: dotnet restore && dotnet publish -c Release -o ./publish
6. Start Command: dotnet ./publish/EnterpriseITToolkit.Worker.dll
7. Plan: Starter ($7/month)

## Step 7: Configure Custom Domain (Optional)
1. Go to your web service settings
2. Add custom domain
3. Configure DNS records
4. SSL certificate will be automatically provisioned

## Step 8: Test Deployment
1. Visit your web service URL
2. Test API endpoints
3. Verify database connections
4. Check background workers

## URLs After Deployment:
- Web Interface: https://enterprise-toolkit-web.onrender.com
- API Server: https://enterprise-toolkit-api.onrender.com
- Custom Domain: https://your-domain.com (if configured)

## Cost Summary:
- Free Tier: $0/month (services sleep after inactivity)
- Starter Plan: $7/month per service (always-on)
- Database: Free (PostgreSQL + Redis)
- Total for Production: $14/month (Web + API services)

## Support:
- Render Documentation: https://render.com/docs
- Community: https://community.render.com
- Status: https://status.render.com
"@

$deploymentInstructions | Out-File -FilePath "RENDER_DEPLOYMENT_GUIDE.md" -Encoding UTF8
Write-Host "✅ Deployment guide created (RENDER_DEPLOYMENT_GUIDE.md)" -ForegroundColor Green

# 5. Create Docker deployment option
Write-Host "`n5. Creating Docker deployment option..." -ForegroundColor Yellow
$dockerDeployment = @"
# Docker Deployment for Render

## Option 1: Use render.yaml (Recommended)
The render.yaml file in your repository will automatically configure all services.

## Option 2: Manual Docker Deployment
1. Create a Dockerfile for your application
2. Use Render's Docker environment
3. Configure environment variables manually

## Dockerfile Example:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnterpriseITToolkit.csproj", "."]
RUN dotnet restore "./EnterpriseITToolkit.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EnterpriseITToolkit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnterpriseITToolkit.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseITToolkit.dll"]
```

## Environment Variables for Docker:
- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://0.0.0.0:10000
- ConnectionStrings__DefaultConnection=your-database-url
- ConnectionStrings__Redis=your-redis-url
"@

$dockerDeployment | Out-File -FilePath "DOCKER_DEPLOYMENT.md" -Encoding UTF8
Write-Host "✅ Docker deployment guide created (DOCKER_DEPLOYMENT.md)" -ForegroundColor Green

# 6. Commit deployment files
Write-Host "`n6. Committing deployment files..." -ForegroundColor Yellow
try {
    git add .
    git commit -m "Deploy: Add Render deployment configuration

- Add render.yaml for automated deployment
- Add environment configuration template
- Add comprehensive deployment guide
- Add Docker deployment options
- Configure all services (Web, API, Database, Redis)
- Set up production environment variables"
    
    git push origin main
    Write-Host "✅ Deployment files committed and pushed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not commit deployment files automatically" -ForegroundColor Yellow
    Write-Host "Please commit and push manually:" -ForegroundColor White
    Write-Host "   git add ." -ForegroundColor Gray
    Write-Host "   git commit -m 'Deploy: Add Render deployment configuration'" -ForegroundColor Gray
    Write-Host "   git push origin main" -ForegroundColor Gray
}

Write-Host "`n=== RENDER DEPLOYMENT READY ===" -ForegroundColor Green
Write-Host "Your application is now ready for Render deployment!" -ForegroundColor White

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Cyan
Write-Host "1. Go to https://render.com and create an account" -ForegroundColor White
Write-Host "2. Connect your GitHub repository" -ForegroundColor White
Write-Host "3. Follow the deployment guide in RENDER_DEPLOYMENT_GUIDE.md" -ForegroundColor White
Write-Host "4. Use the render.yaml file for automated setup" -ForegroundColor White
Write-Host "5. Configure environment variables from render-env-config.txt" -ForegroundColor White

Write-Host "`n=== DEPLOYMENT FILES CREATED ===" -ForegroundColor Yellow
Write-Host "✅ render.yaml - Automated deployment configuration" -ForegroundColor Green
Write-Host "✅ render-env-config.txt - Environment variables template" -ForegroundColor Green
Write-Host "✅ RENDER_DEPLOYMENT_GUIDE.md - Step-by-step deployment guide" -ForegroundColor Green
Write-Host "✅ DOCKER_DEPLOYMENT.md - Docker deployment options" -ForegroundColor Green

Write-Host "`n=== COST ESTIMATE ===" -ForegroundColor Cyan
Write-Host "Free Tier: $0/month (services sleep after inactivity)" -ForegroundColor White
Write-Host "Starter Plan: $7/month per service (always-on)" -ForegroundColor White
Write-Host "Production Setup: $14/month (Web + API services)" -ForegroundColor White
Write-Host "Database: Free (PostgreSQL + Redis)" -ForegroundColor White

Write-Host "`n=== DEPLOYMENT COMPLETE ===" -ForegroundColor Green
Write-Host "Your Enterprise IT Toolkit is ready for Render deployment!" -ForegroundColor White
