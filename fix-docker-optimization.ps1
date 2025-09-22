# Docker Optimization and Security Fix Script
# This script fixes Docker connectivity issues and optimizes performance

Write-Host "=== DOCKER OPTIMIZATION & SECURITY FIX ===" -ForegroundColor Cyan

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires Administrator privileges. Please run as Administrator." -ForegroundColor Red
    exit 1
}

Write-Host "`n1. Stopping all Docker processes..." -ForegroundColor Yellow
try {
    Get-Process -Name "*docker*" -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "✅ Docker processes stopped" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Some Docker processes may still be running" -ForegroundColor Yellow
}

Write-Host "`n2. Starting Docker Desktop..." -ForegroundColor Yellow
try {
    Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe" -WindowStyle Hidden
    Write-Host "✅ Docker Desktop started" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start Docker Desktop" -ForegroundColor Red
    Write-Host "Please start Docker Desktop manually" -ForegroundColor Yellow
}

Write-Host "`n3. Waiting for Docker initialization..." -ForegroundColor Yellow
Write-Host "This may take 2-3 minutes..." -ForegroundColor Cyan
Start-Sleep -Seconds 120

Write-Host "`n4. Testing Docker connectivity..." -ForegroundColor Yellow
try {
    $dockerVersion = docker version --format "{{.Server.Version}}" 2>$null
    if ($dockerVersion) {
        Write-Host "✅ Docker is running (Version: $dockerVersion)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Docker may still be initializing" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Docker connectivity test failed" -ForegroundColor Red
}

Write-Host "`n5. Checking Docker containers..." -ForegroundColor Yellow
try {
    $containers = docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>$null
    if ($containers) {
        Write-Host "✅ Docker containers accessible:" -ForegroundColor Green
        Write-Host $containers
    } else {
        Write-Host "⚠️ No containers found or Docker still initializing" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Cannot access Docker containers" -ForegroundColor Red
}

Write-Host "`n6. Checking Docker images..." -ForegroundColor Yellow
try {
    $images = docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}" 2>$null
    if ($images) {
        Write-Host "✅ Docker images accessible:" -ForegroundColor Green
        Write-Host $images
    } else {
        Write-Host "⚠️ No images found or Docker still initializing" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Cannot access Docker images" -ForegroundColor Red
}

Write-Host "`n7. Checking Docker system usage..." -ForegroundColor Yellow
try {
    $systemDf = docker system df 2>$null
    if ($systemDf) {
        Write-Host "✅ Docker system usage:" -ForegroundColor Green
        Write-Host $systemDf
    } else {
        Write-Host "⚠️ Cannot access Docker system info" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Cannot access Docker system information" -ForegroundColor Red
}

Write-Host "`n8. Docker process monitoring..." -ForegroundColor Yellow
$dockerProcesses = Get-Process -Name "*docker*" -ErrorAction SilentlyContinue | Select-Object ProcessName, Id, CPU, WorkingSet
if ($dockerProcesses) {
    Write-Host "Current Docker processes:" -ForegroundColor Cyan
    $dockerProcesses | Format-Table -AutoSize
} else {
    Write-Host "No Docker processes found" -ForegroundColor Yellow
}

Write-Host "`n=== DOCKER OPTIMIZATION RECOMMENDATIONS ===" -ForegroundColor Cyan

Write-Host "`n📋 MANUAL STEPS REQUIRED:" -ForegroundColor Yellow
Write-Host "1. Open Docker Desktop Settings" -ForegroundColor White
Write-Host "2. Go to Resources > Advanced" -ForegroundColor White
Write-Host "3. Set CPU: 4 cores (reduce from current high usage)" -ForegroundColor White
Write-Host "4. Set Memory: 4GB (reduce from current allocation)" -ForegroundColor White
Write-Host "5. Enable 'Use WSL 2 based engine' if needed" -ForegroundColor White
Write-Host "6. Apply & Restart Docker Desktop" -ForegroundColor White

Write-Host "`n🔧 CLEANUP COMMANDS (run after Docker is stable):" -ForegroundColor Yellow
Write-Host "docker system prune -a -f --volumes" -ForegroundColor White
Write-Host "docker image prune -a -f" -ForegroundColor White
Write-Host "docker container prune -f" -ForegroundColor White

Write-Host "`n🛡️ SECURITY COMMANDS (run after Docker is stable):" -ForegroundColor Yellow
Write-Host "docker scan <image-name>  # Scan for vulnerabilities" -ForegroundColor White
Write-Host "docker pull node:18-alpine3.17  # Update to secure base image" -ForegroundColor White

Write-Host "`n=== SCRIPT COMPLETED ===" -ForegroundColor Green
Write-Host "Docker optimization script completed!" -ForegroundColor Green
Write-Host "Please follow the manual steps above to complete optimization." -ForegroundColor Cyan
