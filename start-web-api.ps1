# Enterprise IT Toolkit Web API Startup Script
Write-Host "Starting Enterprise IT Toolkit Web API..." -ForegroundColor Green

# Check if Docker is running
try {
    docker version | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Build and start the services
Write-Host "Building and starting Docker services..." -ForegroundColor Yellow
docker-compose up --build -d

# Wait for services to start
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if API is responding
Write-Host "Checking API health..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/health" -Method Get -TimeoutSec 10
    Write-Host "✓ API is healthy and responding" -ForegroundColor Green
} catch {
    Write-Host "⚠ API may still be starting up. Please wait a moment and try again." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🚀 Enterprise IT Toolkit Web API is starting up!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Available Services:" -ForegroundColor Cyan
Write-Host "  • Web API: http://localhost:5001" -ForegroundColor White
Write-Host "  • Swagger UI: http://localhost:5001" -ForegroundColor White
Write-Host "  • Health Checks: http://localhost:5001/health" -ForegroundColor White
Write-Host "  • Hangfire Dashboard: http://localhost:5001/hangfire" -ForegroundColor White
Write-Host "  • PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host "  • Redis: localhost:6379" -ForegroundColor White
Write-Host "  • phpMyAdmin: http://localhost:8080" -ForegroundColor White
Write-Host ""
Write-Host "🔐 Default Login Credentials:" -ForegroundColor Cyan
Write-Host "  • Username: admin" -ForegroundColor White
Write-Host "  • Password: admin123" -ForegroundColor White
Write-Host ""
Write-Host "📖 To view logs: docker-compose logs -f enterprise-api" -ForegroundColor Yellow
Write-Host "🛑 To stop: docker-compose down" -ForegroundColor Yellow
