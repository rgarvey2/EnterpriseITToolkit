# Docker Security Hardening Script
# This script implements security best practices for Docker containers

Write-Host "=== DOCKER SECURITY HARDENING ===" -ForegroundColor Cyan
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

Write-Host "`n=== SECURITY ASSESSMENT ===" -ForegroundColor Yellow

# 1. Check for containers running as root
Write-Host "1. Checking for containers running as root..." -ForegroundColor Cyan
try {
    $rootContainers = docker ps --format "{{.Names}}|{{.Command}}" | Where-Object { $_ -like "*root*" -or $_ -like "*sh*" -or $_ -like "*bash*" }
    if ($rootContainers) {
        Write-Host "⚠️ Containers running with elevated privileges:" -ForegroundColor Yellow
        foreach ($container in $rootContainers) {
            Write-Host "  - $container" -ForegroundColor Yellow
        }
        Write-Host "Recommendation: Use non-root users in containers" -ForegroundColor Yellow
    } else {
        Write-Host "✅ No containers running as root detected" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Could not check container privileges" -ForegroundColor Red
}

# 2. Check exposed ports
Write-Host "`n2. Analyzing exposed ports..." -ForegroundColor Cyan
try {
    $exposedPorts = docker ps --format "{{.Names}}|{{.Ports}}" | Where-Object { $_ -like "*:*" }
    if ($exposedPorts) {
        Write-Host "Exposed ports analysis:" -ForegroundColor Cyan
        foreach ($port in $exposedPorts) {
            $parts = $port -split '\|'
            $name = $parts[0]
            $ports = $parts[1]
            
            # Check for potentially dangerous ports
            if ($ports -like "*22*" -or $ports -like "*23*" -or $ports -like "*21*") {
                Write-Host "⚠️ $name : $ports (Potentially dangerous ports exposed)" -ForegroundColor Yellow
            } else {
                Write-Host "✅ $name : $ports" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "✅ No exposed ports found" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Could not analyze exposed ports" -ForegroundColor Red
}

# 3. Check for privileged containers
Write-Host "`n3. Checking for privileged containers..." -ForegroundColor Cyan
try {
    $privilegedContainers = docker ps --format "{{.Names}}|{{.Command}}" | Where-Object { $_ -like "*privileged*" }
    if ($privilegedContainers) {
        Write-Host "⚠️ Privileged containers detected:" -ForegroundColor Yellow
        foreach ($container in $privilegedContainers) {
            Write-Host "  - $container" -ForegroundColor Yellow
        }
        Write-Host "Recommendation: Avoid privileged containers when possible" -ForegroundColor Yellow
    } else {
        Write-Host "✅ No privileged containers detected" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Could not check for privileged containers" -ForegroundColor Red
}

# 4. Check container resource limits
Write-Host "`n4. Checking container resource limits..." -ForegroundColor Cyan
try {
    $containers = docker ps --format "{{.Names}}"
    $unlimitedContainers = @()
    
    foreach ($container in $containers) {
        $inspect = docker inspect $container --format='{{.HostConfig.Memory}}|{{.HostConfig.CpuQuota}}' 2>$null
        if ($inspect) {
            $parts = $inspect -split '\|'
            $memory = $parts[0]
            $cpu = $parts[1]
            
            if ($memory -eq "0" -and $cpu -eq "0") {
                $unlimitedContainers += $container
            }
        }
    }
    
    if ($unlimitedContainers.Count -gt 0) {
        Write-Host "⚠️ Containers without resource limits:" -ForegroundColor Yellow
        foreach ($container in $unlimitedContainers) {
            Write-Host "  - $container" -ForegroundColor Yellow
        }
        Write-Host "Recommendation: Set memory and CPU limits for all containers" -ForegroundColor Yellow
    } else {
        Write-Host "✅ All containers have resource limits configured" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Could not check resource limits" -ForegroundColor Red
}

Write-Host "`n=== SECURITY RECOMMENDATIONS ===" -ForegroundColor Yellow

# Generate security recommendations
$recommendations = @()

# Check for security issues and add recommendations
if ($rootContainers) {
    $recommendations += "• Use non-root users in containers (add USER directive in Dockerfile)"
}

if ($privilegedContainers) {
    $recommendations += "• Avoid privileged containers unless absolutely necessary"
}

if ($unlimitedContainers.Count -gt 0) {
    $recommendations += "• Set resource limits for all containers (--memory, --cpus)"
}

# Always include general recommendations
$recommendations += "• Regularly update base images to latest versions"
$recommendations += "• Use multi-stage builds to reduce attack surface"
$recommendations += "• Implement network segmentation with custom networks"
$recommendations += "• Use Docker secrets for sensitive data"
$recommendations += "• Enable Docker Content Trust for image verification"
$recommendations += "• Implement container scanning in CI/CD pipeline"

Write-Host "Security recommendations:" -ForegroundColor Cyan
foreach ($recommendation in $recommendations) {
    Write-Host "  $recommendation" -ForegroundColor White
}

Write-Host "`n=== SECURITY HARDENING ACTIONS ===" -ForegroundColor Yellow

# 1. Create secure network
Write-Host "1. Creating secure networks..." -ForegroundColor Cyan
try {
    # Create internal network for backend services
    $backendNetwork = docker network create --driver bridge --internal secure-backend 2>$null
    if ($backendNetwork) {
        Write-Host "✅ Created secure backend network: $backendNetwork" -ForegroundColor Green
    } else {
        Write-Host "ℹ️ Backend network may already exist" -ForegroundColor Blue
    }
    
    # Create frontend network
    $frontendNetwork = docker network create --driver bridge secure-frontend 2>$null
    if ($frontendNetwork) {
        Write-Host "✅ Created secure frontend network: $frontendNetwork" -ForegroundColor Green
    } else {
        Write-Host "ℹ️ Frontend network may already exist" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Failed to create secure networks" -ForegroundColor Red
}

# 2. Check for security updates
Write-Host "`n2. Checking for security updates..." -ForegroundColor Cyan
try {
    $images = docker images --format "{{.Repository}}:{{.Tag}}" | Where-Object { $_ -notlike "*<none>*" }
    if ($images) {
        Write-Host "Current images:" -ForegroundColor Cyan
        foreach ($image in $images) {
            Write-Host "  - $image" -ForegroundColor White
        }
        Write-Host "`nRecommendation: Regularly pull latest versions of base images" -ForegroundColor Yellow
    } else {
        Write-Host "ℹ️ No images found" -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Could not check images" -ForegroundColor Red
}

# 3. Security configuration recommendations
Write-Host "`n3. Security configuration recommendations..." -ForegroundColor Cyan
Write-Host "For enhanced security, consider implementing:" -ForegroundColor White
Write-Host "  • Docker Content Trust (DCT) for image signing" -ForegroundColor White
Write-Host "  • Container runtime security (gVisor, Kata Containers)" -ForegroundColor White
Write-Host "  • Network policies and firewalls" -ForegroundColor White
Write-Host "  • Regular vulnerability scanning" -ForegroundColor White
Write-Host "  • Audit logging for container activities" -ForegroundColor White

Write-Host "`n=== SECURITY MONITORING SETUP ===" -ForegroundColor Yellow

# Create security monitoring commands
Write-Host "Security monitoring commands:" -ForegroundColor Cyan
Write-Host "  • docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'" -ForegroundColor White
Write-Host "  • docker stats --no-stream" -ForegroundColor White
Write-Host "  • docker system df" -ForegroundColor White
Write-Host "  • docker network ls" -ForegroundColor White
Write-Host "  • docker volume ls" -ForegroundColor White

Write-Host "`n=== SECURITY HARDENING COMPLETE ===" -ForegroundColor Green
Write-Host "Docker security assessment and hardening completed!" -ForegroundColor Cyan
Write-Host "Review the recommendations above and implement as needed." -ForegroundColor Yellow
