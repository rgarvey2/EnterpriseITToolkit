# PowerShell script to start the Enterprise IT Toolkit with Docker
Write-Host "🚀 Starting Enterprise IT Toolkit with Docker..." -ForegroundColor Green

# Check if Docker is running
try {
    docker version | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Stop any existing containers
Write-Host "🛑 Stopping existing containers..." -ForegroundColor Yellow
docker-compose -f docker-compose.selfhost.yml down

# Build and start the containers
Write-Host "🔨 Building and starting containers..." -ForegroundColor Yellow
docker-compose -f docker-compose.selfhost.yml up --build -d

# Wait for services to start
Write-Host "⏳ Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if services are running
Write-Host "🔍 Checking service status..." -ForegroundColor Yellow
docker-compose -f docker-compose.selfhost.yml ps

# Test API connection
Write-Host "🧪 Testing API connection..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:3000/health" -Method Get
    Write-Host "✅ API server is responding: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "⚠️ API server not responding yet, may need more time to start" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎉 Enterprise IT Toolkit is now running!" -ForegroundColor Green
Write-Host "📱 Web App: http://localhost:8080" -ForegroundColor Cyan
Write-Host "🔌 API Server: http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop the services, run: docker-compose -f docker-compose.selfhost.yml down" -ForegroundColor Yellow
Write-Host "To view logs, run: docker-compose -f docker-compose.selfhost.yml logs -f" -ForegroundColor Yellow
