# Docker Security and Performance Monitoring Script
# This script monitors Docker containers, security, and performance

Write-Host "=== DOCKER SECURITY & PERFORMANCE MONITOR ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Check if Docker is running
try {
    $dockerVersion = docker version --format "{{.Server.Version}}" 2>$null
    if ($dockerVersion) {
        Write-Host "✅ Docker is running (Version: $dockerVersion)" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker is not accessible" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Docker is not running" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== CONTAINER STATUS ===" -ForegroundColor Yellow

# Get container status
$containers = docker ps -a --format "{{.Names}}|{{.Status}}|{{.Ports}}" 2>$null
if ($containers) {
    foreach ($container in $containers) {
        $parts = $container -split '\|'
        $name = $parts[0]
        $status = $parts[1]
        $ports = $parts[2]
        
        if ($status -like "*Up*") {
            Write-Host "✅ $name : $status" -ForegroundColor Green
        } else {
            Write-Host "❌ $name : $status" -ForegroundColor Red
        }
    }
} else {
    Write-Host "⚠️ No containers found" -ForegroundColor Yellow
}

Write-Host "`n=== HEALTH CHECKS ===" -ForegroundColor Yellow

# Check health status
$containerNames = @("enterprise-php-admin", "enterprise-api-server", "enterprise-postgres", "enterprise-redis", "enterprise-grafana")
foreach ($containerName in $containerNames) {
    try {
        $healthStatus = docker inspect $containerName --format='{{.State.Health.Status}}' 2>$null
        if ($healthStatus) {
            if ($healthStatus -eq "healthy") {
                Write-Host "✅ $containerName : $healthStatus" -ForegroundColor Green
            } elseif ($healthStatus -eq "unhealthy") {
                Write-Host "❌ $containerName : $healthStatus" -ForegroundColor Red
            } else {
                Write-Host "⚠️ $containerName : $healthStatus" -ForegroundColor Yellow
            }
        } else {
            Write-Host "⚠️ $containerName : No health check configured" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ $containerName : Health check failed" -ForegroundColor Red
    }
}

Write-Host "`n=== RESOURCE USAGE ===" -ForegroundColor Yellow

# Get resource usage
try {
    $stats = docker stats --no-stream --format "{{.Container}}|{{.CPUPerc}}|{{.MemUsage}}|{{.NetIO}}|{{.BlockIO}}" 2>$null
    if ($stats) {
        Write-Host "Container Name`tCPU%`tMemory Usage`tNetwork I/O`tBlock I/O" -ForegroundColor Cyan
        Write-Host "----------------------------------------------------------------" -ForegroundColor Gray
        foreach ($stat in $stats) {
            $parts = $stat -split '\|'
            $container = $parts[0]
            $cpu = $parts[1]
            $memory = $parts[2]
            $netIO = $parts[3]
            $blockIO = $parts[4]
            
            # Color code based on CPU usage
            if ([double]($cpu -replace '%', '') -gt 50) {
                $cpuColor = "Red"
            } elseif ([double]($cpu -replace '%', '') -gt 25) {
                $cpuColor = "Yellow"
            } else {
                $cpuColor = "Green"
            }
            
            Write-Host "$container`t$cpu`t$memory`t$netIO`t$blockIO" -ForegroundColor $cpuColor
        }
    } else {
        Write-Host "⚠️ Could not retrieve resource statistics" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Failed to get resource statistics" -ForegroundColor Red
}

Write-Host "`n=== SECURITY CHECKS ===" -ForegroundColor Yellow

# Check for running containers with root privileges
try {
    $rootContainers = docker ps --format "{{.Names}}|{{.Command}}" | Where-Object { $_ -like "*root*" -or $_ -like "*sh*" }
    if ($rootContainers) {
        Write-Host "⚠️ Containers running with elevated privileges:" -ForegroundColor Yellow
        foreach ($container in $rootContainers) {
            Write-Host "  - $container" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✅ No containers running with root privileges detected" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Could not check container privileges" -ForegroundColor Red
}

# Check exposed ports
Write-Host "`n=== EXPOSED PORTS ===" -ForegroundColor Yellow
try {
    $exposedPorts = docker ps --format "{{.Names}}|{{.Ports}}" | Where-Object { $_ -like "*:*" }
    if ($exposedPorts) {
        foreach ($port in $exposedPorts) {
            $parts = $port -split '\|'
            $name = $parts[0]
            $ports = $parts[1]
            Write-Host "✅ $name : $ports" -ForegroundColor Green
        }
    } else {
        Write-Host "⚠️ No exposed ports found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Could not check exposed ports" -ForegroundColor Red
}

Write-Host "`n=== ACCESSIBILITY TESTS ===" -ForegroundColor Yellow

# Test container accessibility
$testUrls = @(
    @{Name="PHP Admin"; Url="http://localhost:8080"},
    @{Name="API Server"; Url="http://localhost:5001"},
    @{Name="Grafana"; Url="http://localhost:3000"}
)

foreach ($test in $testUrls) {
    try {
        $response = Invoke-WebRequest -Uri $test.Url -TimeoutSec 3 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($test.Name) : Accessible (HTTP $($response.StatusCode))" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $($test.Name) : HTTP $($response.StatusCode)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ $($test.Name) : Not accessible - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== SYSTEM RESOURCES ===" -ForegroundColor Yellow

# Check system resources
try {
    $cpuUsage = Get-Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    $memoryAvailable = Get-Counter "\Memory\Available MBytes" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    
    Write-Host "System CPU Usage: $([math]::Round($cpuUsage, 2))%" -ForegroundColor $(if ($cpuUsage -gt 80) { "Red" } elseif ($cpuUsage -gt 60) { "Yellow" } else { "Green" })
    Write-Host "Available Memory: $([math]::Round($memoryAvailable, 0)) MB" -ForegroundColor $(if ($memoryAvailable -lt 1000) { "Red" } elseif ($memoryAvailable -lt 2000) { "Yellow" } else { "Green" })
} catch {
    Write-Host "❌ Could not retrieve system resource information" -ForegroundColor Red
}

Write-Host "`n=== RECOMMENDATIONS ===" -ForegroundColor Cyan

# Generate recommendations based on findings
$recommendations = @()

# Check for unhealthy containers
$unhealthyContainers = docker ps --format "{{.Names}}|{{.Status}}" | Where-Object { $_ -like "*unhealthy*" }
if ($unhealthyContainers) {
    $recommendations += "• Investigate unhealthy containers and fix health check issues"
}

# Check for high CPU usage
$highCpuContainers = docker stats --no-stream --format "{{.Container}}|{{.CPUPerc}}" | Where-Object { [double]($_ -split '\|')[1].Replace('%', '') -gt 50 }
if ($highCpuContainers) {
    $recommendations += "• Monitor containers with high CPU usage and consider resource limits"
}

# Check for inaccessible services
$inaccessibleServices = @()
foreach ($test in $testUrls) {
    try {
        $response = Invoke-WebRequest -Uri $test.Url -TimeoutSec 2 -ErrorAction Stop
    } catch {
        $inaccessibleServices += $test.Name
    }
}
if ($inaccessibleServices.Count -gt 0) {
    $recommendations += "• Fix accessibility issues for: $($inaccessibleServices -join ', ')"
}

if ($recommendations.Count -eq 0) {
    Write-Host "✅ No immediate recommendations - system is running optimally" -ForegroundColor Green
} else {
    foreach ($recommendation in $recommendations) {
        Write-Host $recommendation -ForegroundColor Yellow
    }
}

Write-Host "`n=== MONITORING COMPLETE ===" -ForegroundColor Green
Write-Host "Run this script regularly to monitor Docker health and security" -ForegroundColor Cyan
