# Performance Monitoring Script
# Run this script to monitor system performance

Write-Host "=== SYSTEM PERFORMANCE MONITOR ===" -ForegroundColor Cyan

# CPU Usage
$cpuUsage = Get-Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
Write-Host "CPU Usage: $([math]::Round($cpuUsage, 2))%" -ForegroundColor $(if ($cpuUsage -gt 80) { "Red" } elseif ($cpuUsage -gt 60) { "Yellow" } else { "Green" })

# Memory Usage
$memoryAvailable = Get-Counter "\Memory\Available MBytes" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
Write-Host "Available Memory: $([math]::Round($memoryAvailable, 0)) MB" -ForegroundColor $(if ($memoryAvailable -lt 1000) { "Red" } elseif ($memoryAvailable -lt 2000) { "Yellow" } else { "Green" })

# Disk Usage
$diskUsage = Get-Counter "\PhysicalDisk(_Total)\% Disk Time" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
Write-Host "Disk Usage: $([math]::Round($diskUsage, 2))%" -ForegroundColor $(if ($diskUsage -gt 80) { "Red" } elseif ($diskUsage -gt 60) { "Yellow" } else { "Green" })

# Docker Status
try {
    $dockerStats = docker stats --no-stream --format "{{.Container}}|{{.CPUPerc}}|{{.MemUsage}}" 2>$null
    if ($dockerStats) {
        Write-Host "
Docker Container Performance:" -ForegroundColor Cyan
        foreach ($stat in $dockerStats) {
            $parts = $stat -split '\|'
            $container = $parts[0]
            $cpu = $parts[1]
            $memory = $parts[2]
            Write-Host "  $container : CPU $cpu, Memory $memory" -ForegroundColor White
        }
    }
} catch {
    Write-Host "Docker not accessible" -ForegroundColor Yellow
}
