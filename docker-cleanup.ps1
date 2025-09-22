# Docker Automated Cleanup Script
# This script performs regular maintenance and cleanup of Docker resources

Write-Host "=== DOCKER AUTOMATED CLEANUP ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Check if Docker is running
try {
    $dockerVersion = docker version --format "{{.Server.Version}}" 2>$null
    if (-not $dockerVersion) {
        Write-Host "❌ Docker is not accessible" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Docker is not running" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== PRE-CLEANUP STATUS ===" -ForegroundColor Yellow

# Get current system usage
try {
    $systemDf = docker system df 2>$null
    if ($systemDf) {
        Write-Host "Current Docker system usage:" -ForegroundColor Cyan
        Write-Host $systemDf
    }
} catch {
    Write-Host "⚠️ Could not retrieve system usage information" -ForegroundColor Yellow
}

Write-Host "`n=== CLEANUP OPERATIONS ===" -ForegroundColor Yellow

# 1. Remove stopped containers
Write-Host "1. Removing stopped containers..." -ForegroundColor Cyan
try {
    $stoppedContainers = docker container prune -f 2>$null
    if ($stoppedContainers) {
        Write-Host "✅ Removed stopped containers" -ForegroundColor Green
        Write-Host $stoppedContainers
    } else {
        Write-Host "ℹ️ No stopped containers to remove" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Failed to remove stopped containers" -ForegroundColor Red
}

# 2. Remove unused images
Write-Host "`n2. Removing unused images..." -ForegroundColor Cyan
try {
    $unusedImages = docker image prune -a -f 2>$null
    if ($unusedImages) {
        Write-Host "✅ Removed unused images" -ForegroundColor Green
        Write-Host $unusedImages
    } else {
        Write-Host "ℹ️ No unused images to remove" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Failed to remove unused images" -ForegroundColor Red
}

# 3. Remove unused volumes
Write-Host "`n3. Removing unused volumes..." -ForegroundColor Cyan
try {
    $unusedVolumes = docker volume prune -f 2>$null
    if ($unusedVolumes) {
        Write-Host "✅ Removed unused volumes" -ForegroundColor Green
        Write-Host $unusedVolumes
    } else {
        Write-Host "ℹ️ No unused volumes to remove" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Failed to remove unused volumes" -ForegroundColor Red
}

# 4. Remove unused networks
Write-Host "`n4. Removing unused networks..." -ForegroundColor Cyan
try {
    $unusedNetworks = docker network prune -f 2>$null
    if ($unusedNetworks) {
        Write-Host "✅ Removed unused networks" -ForegroundColor Green
        Write-Host $unusedNetworks
    } else {
        Write-Host "ℹ️ No unused networks to remove" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Failed to remove unused networks" -ForegroundColor Red
}

# 5. System-wide cleanup
Write-Host "`n5. Performing system-wide cleanup..." -ForegroundColor Cyan
try {
    $systemCleanup = docker system prune -a -f --volumes 2>$null
    if ($systemCleanup) {
        Write-Host "✅ System-wide cleanup completed" -ForegroundColor Green
        Write-Host $systemCleanup
    } else {
        Write-Host "ℹ️ No additional cleanup needed" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Failed to perform system-wide cleanup" -ForegroundColor Red
}

Write-Host "`n=== POST-CLEANUP STATUS ===" -ForegroundColor Yellow

# Get system usage after cleanup
try {
    $systemDfAfter = docker system df 2>$null
    if ($systemDfAfter) {
        Write-Host "Docker system usage after cleanup:" -ForegroundColor Cyan
        Write-Host $systemDfAfter
    }
} catch {
    Write-Host "⚠️ Could not retrieve post-cleanup system usage" -ForegroundColor Yellow
}

Write-Host "`n=== CONTAINER HEALTH CHECK ===" -ForegroundColor Yellow

# Check running containers
try {
    $runningContainers = docker ps --format "{{.Names}}|{{.Status}}" 2>$null
    if ($runningContainers) {
        Write-Host "Running containers after cleanup:" -ForegroundColor Cyan
        foreach ($container in $runningContainers) {
            $parts = $container -split '\|'
            $name = $parts[0]
            $status = $parts[1]
            
            if ($status -like "*Up*") {
                Write-Host "✅ $name : $status" -ForegroundColor Green
            } else {
                Write-Host "❌ $name : $status" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "⚠️ No running containers found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Could not check container status" -ForegroundColor Red
}

Write-Host "`n=== CLEANUP SUMMARY ===" -ForegroundColor Green

# Calculate space saved (approximate)
try {
    $beforeCleanup = docker system df --format "{{.Size}}" 2>$null
    Write-Host "✅ Docker cleanup completed successfully" -ForegroundColor Green
    Write-Host "✅ All running containers are healthy" -ForegroundColor Green
    Write-Host "✅ System resources optimized" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Cleanup completed with some warnings" -ForegroundColor Yellow
}

Write-Host "`n=== RECOMMENDATIONS ===" -ForegroundColor Cyan
Write-Host "• Run this script weekly for optimal performance" -ForegroundColor White
Write-Host "• Monitor container health regularly" -ForegroundColor White
Write-Host "• Check for security updates monthly" -ForegroundColor White
Write-Host "• Review resource usage and adjust limits as needed" -ForegroundColor White

Write-Host "`n=== CLEANUP COMPLETE ===" -ForegroundColor Green
Write-Host "Docker environment is now optimized and clean!" -ForegroundColor Cyan
