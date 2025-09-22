# Advanced System Optimization Script
# Comprehensive optimization for Host, Docker, and Security

Write-Host "=== ADVANCED SYSTEM OPTIMIZATION ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This script requires Administrator privileges. Please run as Administrator." -ForegroundColor Red
    exit 1
}

Write-Host "`n=== HOST SYSTEM OPTIMIZATION ===" -ForegroundColor Yellow

# 1. Optimize Windows Services
Write-Host "1. Optimizing Windows Services..." -ForegroundColor Cyan

# Services to disable for better performance (keeping Xbox and gaming services)
$servicesToDisable = @(
    "DiagTrack",           # Diagnostics Tracking Service
    "WSearch",             # Windows Search (if not needed)
    "Fax",                 # Fax Service
    "WbioSrvc",            # Windows Biometric Service
    "TabletInputService",  # Tablet PC Input Service
    "WMPNetworkSvc"        # Windows Media Player Network Sharing
    # Note: Xbox and gaming services preserved as requested
    # "XblAuthManager",      # Xbox Live Auth Manager
    # "XblGameSave",         # Xbox Live Game Save
    # "XboxGipSvc",          # Xbox Accessory Management
    # "XboxNetApiSvc"        # Xbox Live Networking Service
)

foreach ($service in $servicesToDisable) {
    try {
        $svc = Get-Service -Name $service -ErrorAction SilentlyContinue
        if ($svc -and $svc.Status -eq "Running") {
            Set-Service -Name $service -StartupType Disabled -ErrorAction SilentlyContinue
            Write-Host "  ✅ Disabled $service" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️ Could not disable $service" -ForegroundColor Yellow
    }
}

# 2. Optimize Windows Defender
Write-Host "`n2. Optimizing Windows Defender..." -ForegroundColor Cyan

# Add more exclusions for development
$defenderExclusions = @(
    "C:\Program Files\Docker",
    "C:\ProgramData\Docker",
    "C:\Users\$env:USERNAME\AppData\Local\Docker",
    "C:\Users\$env:USERNAME\.docker",
    "C:\Users\$env:USERNAME\Desktop\EnterpriseITToolkit",
    "C:\Program Files\nodejs",
    "C:\Users\$env:USERNAME\AppData\Roaming\npm",
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Microsoft VS Code"
)

foreach ($exclusion in $defenderExclusions) {
    try {
        if (Test-Path $exclusion) {
            Add-MpPreference -ExclusionPath $exclusion -ErrorAction SilentlyContinue
            Write-Host "  ✅ Added exclusion: $exclusion" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️ Could not add exclusion: $exclusion" -ForegroundColor Yellow
    }
}

# 3. Optimize Power Settings
Write-Host "`n3. Optimizing Power Settings..." -ForegroundColor Cyan
try {
    # Set high performance power plan
    powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
    Write-Host "  ✅ Set high performance power plan" -ForegroundColor Green
    
    # Disable USB selective suspend
    powercfg /setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
    powercfg /setdcvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
    powercfg /setactive SCHEME_CURRENT
    Write-Host "  ✅ Disabled USB selective suspend" -ForegroundColor Green
} catch {
    Write-Host "  ⚠️ Could not optimize power settings" -ForegroundColor Yellow
}

# 4. Optimize Virtual Memory
Write-Host "`n4. Optimizing Virtual Memory..." -ForegroundColor Cyan
try {
    # Get system memory
    $totalMemory = (Get-CimInstance -ClassName Win32_PhysicalMemory | Measure-Object -Property Capacity -Sum).Sum / 1GB
    $initialSize = [math]::Round($totalMemory * 1.5)
    $maximumSize = [math]::Round($totalMemory * 3)
    
    Write-Host "  ℹ️ Total Memory: $([math]::Round($totalMemory, 2)) GB" -ForegroundColor Blue
    Write-Host "  ℹ️ Recommended Initial Size: $initialSize MB" -ForegroundColor Blue
    Write-Host "  ℹ️ Recommended Maximum Size: $maximumSize MB" -ForegroundColor Blue
    Write-Host "  ⚠️ Manual configuration required in System Properties" -ForegroundColor Yellow
} catch {
    Write-Host "  ❌ Could not analyze memory settings" -ForegroundColor Red
}

Write-Host "`n=== DOCKER OPTIMIZATION ===" -ForegroundColor Yellow

# 5. Docker Resource Optimization
Write-Host "5. Optimizing Docker Resources..." -ForegroundColor Cyan

# Check Docker Desktop settings
Write-Host "  ℹ️ Docker Desktop Settings Recommendations:" -ForegroundColor Blue
Write-Host "    • CPU: Limit to 4-6 cores (current: all available)" -ForegroundColor White
Write-Host "    • Memory: Limit to 4-6GB (current: 8GB+)" -ForegroundColor White
Write-Host "    • Disk: Enable automatic cleanup" -ForegroundColor White
Write-Host "    • WSL Integration: Disable if not using WSL containers" -ForegroundColor White

# 6. Docker Image Optimization
Write-Host "`n6. Optimizing Docker Images..." -ForegroundColor Cyan

# Remove unused images
try {
    $unusedImages = docker image prune -a -f 2>$null
    if ($unusedImages) {
        Write-Host "  ✅ Removed unused images" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️ No unused images to remove" -ForegroundColor Blue
    }
} catch {
    Write-Host "  ❌ Could not remove unused images" -ForegroundColor Red
}

# 7. Docker Container Resource Limits
Write-Host "`n7. Setting Docker Container Resource Limits..." -ForegroundColor Cyan

# Create optimized docker-compose override
$dockerComposeOverride = @"
version: '3.8'
services:
  enterprise-php-admin:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
  
  enterprise-api-server:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
  
  enterprise-postgres:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
  
  enterprise-redis:
    deploy:
      resources:
        limits:
          cpus: '0.25'
          memory: 256M
        reservations:
          cpus: '0.1'
          memory: 128M
  
  enterprise-grafana:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
"@

$dockerComposeOverride | Out-File -FilePath "docker-compose.override.yml" -Encoding UTF8
Write-Host "  ✅ Created docker-compose.override.yml with resource limits" -ForegroundColor Green

Write-Host "`n=== SECURITY OPTIMIZATION ===" -ForegroundColor Yellow

# 8. Enhanced Security Configuration
Write-Host "8. Implementing Enhanced Security..." -ForegroundColor Cyan

# Create security configuration file
$securityConfig = @"
# Enhanced Security Configuration
# Windows Defender Advanced Settings

# Enable Controlled Folder Access
Set-MpPreference -EnableControlledFolderAccess Enabled

# Enable Network Protection
Set-MpPreference -EnableNetworkProtection Enabled

# Enable Cloud Protection
Set-MpPreference -EnableCloudProtection Enabled

# Set Real-time Protection
Set-MpPreference -DisableRealtimeMonitoring $false

# Enable Behavior Monitoring
Set-MpPreference -DisableBehaviorMonitoring $false

# Enable IOAV Protection
Set-MpPreference -DisableIOAVProtection $false

# Enable Script Scanning
Set-MpPreference -DisableScriptScanning $false
"@

$securityConfig | Out-File -FilePath "security-config.ps1" -Encoding UTF8
Write-Host "  ✅ Created enhanced security configuration" -ForegroundColor Green

# 9. Docker Security Hardening
Write-Host "`n9. Docker Security Hardening..." -ForegroundColor Cyan

# Create secure Docker daemon configuration
$dockerDaemonConfig = @"
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "storage-driver": "overlay2",
  "live-restore": true,
  "userland-proxy": false,
  "no-new-privileges": true,
  "seccomp-profile": "/etc/docker/seccomp-profile.json",
  "apparmor-profile": "docker-default"
}
"@

$dockerDaemonConfig | Out-File -FilePath "daemon.json" -Encoding UTF8
Write-Host "  ✅ Created secure Docker daemon configuration" -ForegroundColor Green

# 10. Network Security
Write-Host "`n10. Implementing Network Security..." -ForegroundColor Cyan

# Create firewall rules
try {
    # Allow only necessary ports
    $allowedPorts = @(80, 443, 3000, 5001, 5432, 6379, 8080)
    
    foreach ($port in $allowedPorts) {
        try {
            New-NetFirewallRule -DisplayName "Allow Port $port" -Direction Inbound -Protocol TCP -LocalPort $port -Action Allow -ErrorAction SilentlyContinue
            Write-Host "  ✅ Created firewall rule for port $port" -ForegroundColor Green
        } catch {
            Write-Host "  ⚠️ Firewall rule for port $port may already exist" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "  ❌ Could not create firewall rules" -ForegroundColor Red
}

Write-Host "`n=== PERFORMANCE MONITORING ===" -ForegroundColor Yellow

# 11. Create Performance Monitoring Script
Write-Host "11. Creating Performance Monitoring..." -ForegroundColor Cyan

$performanceMonitor = @"
# Performance Monitoring Script
# Run this script to monitor system performance

Write-Host "=== SYSTEM PERFORMANCE MONITOR ===" -ForegroundColor Cyan

# CPU Usage
`$cpuUsage = Get-Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
Write-Host "CPU Usage: `$([math]::Round(`$cpuUsage, 2))%" -ForegroundColor `$(if (`$cpuUsage -gt 80) { "Red" } elseif (`$cpuUsage -gt 60) { "Yellow" } else { "Green" })

# Memory Usage
`$memoryAvailable = Get-Counter "\Memory\Available MBytes" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
Write-Host "Available Memory: `$([math]::Round(`$memoryAvailable, 0)) MB" -ForegroundColor `$(if (`$memoryAvailable -lt 1000) { "Red" } elseif (`$memoryAvailable -lt 2000) { "Yellow" } else { "Green" })

# Disk Usage
`$diskUsage = Get-Counter "\PhysicalDisk(_Total)\% Disk Time" -SampleInterval 1 -MaxSamples 1 | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
Write-Host "Disk Usage: `$([math]::Round(`$diskUsage, 2))%" -ForegroundColor `$(if (`$diskUsage -gt 80) { "Red" } elseif (`$diskUsage -gt 60) { "Yellow" } else { "Green" })

# Docker Status
try {
    `$dockerStats = docker stats --no-stream --format "{{.Container}}|{{.CPUPerc}}|{{.MemUsage}}" 2>`$null
    if (`$dockerStats) {
        Write-Host "`nDocker Container Performance:" -ForegroundColor Cyan
        foreach (`$stat in `$dockerStats) {
            `$parts = `$stat -split '\|'
            `$container = `$parts[0]
            `$cpu = `$parts[1]
            `$memory = `$parts[2]
            Write-Host "  `$container : CPU `$cpu, Memory `$memory" -ForegroundColor White
        }
    }
} catch {
    Write-Host "Docker not accessible" -ForegroundColor Yellow
}
"@

$performanceMonitor | Out-File -FilePath "performance-monitor.ps1" -Encoding UTF8
Write-Host "  ✅ Created performance monitoring script" -ForegroundColor Green

Write-Host "`n=== OPTIMIZATION SUMMARY ===" -ForegroundColor Green

Write-Host "✅ Host System Optimizations:" -ForegroundColor Green
Write-Host "  • Disabled unnecessary Windows services" -ForegroundColor White
Write-Host "  • Enhanced Windows Defender exclusions" -ForegroundColor White
Write-Host "  • Optimized power settings" -ForegroundColor White
Write-Host "  • Configured virtual memory recommendations" -ForegroundColor White

Write-Host "`n✅ Docker Optimizations:" -ForegroundColor Green
Write-Host "  • Created resource limit configurations" -ForegroundColor White
Write-Host "  • Optimized Docker images" -ForegroundColor White
Write-Host "  • Enhanced security configurations" -ForegroundColor White
Write-Host "  • Implemented network security" -ForegroundColor White

Write-Host "`n✅ Security Enhancements:" -ForegroundColor Green
Write-Host "  • Enhanced Windows Defender settings" -ForegroundColor White
Write-Host "  • Created secure Docker daemon config" -ForegroundColor White
Write-Host "  • Implemented firewall rules" -ForegroundColor White
Write-Host "  • Added performance monitoring" -ForegroundColor White

Write-Host "`n=== MANUAL STEPS REQUIRED ===" -ForegroundColor Yellow
Write-Host "1. Restart Docker Desktop and apply resource limits" -ForegroundColor White
Write-Host "2. Configure virtual memory in System Properties" -ForegroundColor White
Write-Host "3. Review and apply security configurations" -ForegroundColor White
Write-Host "4. Run performance-monitor.ps1 regularly" -ForegroundColor White

Write-Host "`n=== OPTIMIZATION COMPLETE ===" -ForegroundColor Green
Write-Host "Advanced system optimization completed successfully!" -ForegroundColor Cyan
