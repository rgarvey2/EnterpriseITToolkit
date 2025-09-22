# Docker Desktop Configuration Script
# This script helps configure Docker Desktop settings for optimal performance

Write-Host "=== DOCKER DESKTOP CONFIGURATION ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Check if Docker Desktop is running
Write-Host "`nChecking Docker Desktop Status..." -ForegroundColor Yellow
try {
    $dockerInfo = docker info 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Docker Desktop is running" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Desktop is not running or not accessible" -ForegroundColor Red
        Write-Host "Please start Docker Desktop first" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Docker Desktop is not installed or not in PATH" -ForegroundColor Red
}

# Get system information for recommendations
$totalMemory = [math]::Round((Get-WmiObject -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
$cpuCores = (Get-WmiObject -Class Win32_Processor).NumberOfCores

Write-Host "`nSystem Information:" -ForegroundColor Yellow
Write-Host "Total Memory: $totalMemory GB" -ForegroundColor White
Write-Host "CPU Cores: $cpuCores" -ForegroundColor White

# Calculate recommended Docker settings
$recommendedMemory = [math]::Min([math]::Max($totalMemory * 0.25, 4), 8)  # 25% of RAM, min 4GB, max 8GB
$recommendedCpu = [math]::Min([math]::Max($cpuCores * 0.5, 2), 6)  # 50% of cores, min 2, max 6

Write-Host "`nRecommended Docker Desktop Settings:" -ForegroundColor Yellow
Write-Host "Memory Limit: $recommendedMemory GB" -ForegroundColor Green
Write-Host "CPU Limit: $recommendedCpu cores" -ForegroundColor Green

Write-Host "`n=== MANUAL CONFIGURATION STEPS ===" -ForegroundColor Cyan
Write-Host "To configure Docker Desktop settings:" -ForegroundColor Yellow
Write-Host "1. Right-click on Docker Desktop icon in system tray" -ForegroundColor White
Write-Host "2. Select 'Settings' or 'Preferences'" -ForegroundColor White
Write-Host "3. Go to 'Resources' section" -ForegroundColor White
Write-Host "4. Configure the following settings:" -ForegroundColor White
Write-Host "   - Memory: Set to $recommendedMemory GB" -ForegroundColor Green
Write-Host "   - CPUs: Set to $recommendedCpu cores" -ForegroundColor Green
Write-Host "   - Disk image size: Set to 64GB (or as needed)" -ForegroundColor White
Write-Host "5. Go to 'General' section" -ForegroundColor White
Write-Host "6. Enable 'Use WSL 2 based engine' (if available)" -ForegroundColor White
Write-Host "7. Go to 'Resources' → 'WSL Integration'" -ForegroundColor White
Write-Host "8. Disable WSL integration for unused distributions" -ForegroundColor White
Write-Host "9. Go to 'Resources' → 'Advanced'" -ForegroundColor White
Write-Host "10. Enable 'Enable file sharing' for necessary drives only" -ForegroundColor White
Write-Host "11. Click 'Apply & Restart'" -ForegroundColor White

Write-Host "`n=== DOCKER DESKTOP SETTINGS.JSON CONFIGURATION ===" -ForegroundColor Cyan
Write-Host "Alternative method using settings file:" -ForegroundColor Yellow

# Get Docker Desktop settings file path
$dockerSettingsPath = "$env:APPDATA\Docker\settings.json"
Write-Host "`nDocker Desktop settings file location:" -ForegroundColor White
Write-Host $dockerSettingsPath -ForegroundColor Gray

Write-Host "`nRecommended settings.json configuration:" -ForegroundColor Yellow
$recommendedSettings = @{
    "memoryMiB" = $recommendedMemory * 1024
    "cpus" = $recommendedCpu
    "diskSizeMiB" = 65536
    "vmType" = "wsl2"
    "wslEngineEnabled" = $true
    "fileSharingDirectories" = @(
        "C:\\Users\\$env:USERNAME\\Desktop",
        "C:\\Users\\$env:USERNAME\\Documents"
    )
    "experimental" = $false
    "autoStart" = $true
    "startOnLogin" = $true
    "analytics" = $false
    "crashReporting" = $false
    "updates" = @{
        "enabled" = $true
        "channel" = "stable"
    }
}

Write-Host ($recommendedSettings | ConvertTo-Json -Depth 3) -ForegroundColor Gray

Write-Host "`n=== AUTOMATED CONFIGURATION (ADVANCED) ===" -ForegroundColor Cyan
Write-Host "WARNING: This will modify Docker Desktop settings. Use with caution!" -ForegroundColor Red
$confirm = Read-Host "Do you want to attempt automated configuration? (y/N)"

if ($confirm -eq 'y' -or $confirm -eq 'Y') {
    Write-Host "`nAttempting automated configuration..." -ForegroundColor Yellow
    
    try {
        # Check if Docker Desktop is running
        if (-not (Get-Process "Docker Desktop" -ErrorAction SilentlyContinue)) {
            Write-Host "❌ Docker Desktop is not running. Please start it first." -ForegroundColor Red
            exit 1
        }
        
        # Create backup of current settings
        if (Test-Path $dockerSettingsPath) {
            $backupPath = "$dockerSettingsPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item $dockerSettingsPath $backupPath
            Write-Host "✅ Created backup of current settings: $backupPath" -ForegroundColor Green
        }
        
        # Create new settings
        $newSettings = $recommendedSettings | ConvertTo-Json -Depth 3
        $newSettings | Out-File -FilePath $dockerSettingsPath -Encoding UTF8
        
        Write-Host "✅ Docker Desktop settings configured successfully!" -ForegroundColor Green
        Write-Host "Memory: $recommendedMemory GB" -ForegroundColor Green
        Write-Host "CPU: $recommendedCpu cores" -ForegroundColor Green
        Write-Host "`n⚠️  RESTART REQUIRED: Please restart Docker Desktop for changes to take effect" -ForegroundColor Yellow
        
    } catch {
        Write-Host "❌ Error during automated configuration: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Please use manual configuration steps above" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nUsing manual configuration. Please follow the steps above." -ForegroundColor Yellow
}

Write-Host "`n=== DOCKER OPTIMIZATION TIPS ===" -ForegroundColor Cyan
Write-Host "Additional optimization recommendations:" -ForegroundColor Yellow
Write-Host "• Enable 'Use the WSL 2 based engine' for better performance" -ForegroundColor White
Write-Host "• Disable unused WSL distributions in WSL Integration" -ForegroundColor White
Write-Host "• Enable 'Enable file sharing' only for necessary drives" -ForegroundColor White
Write-Host "• Set 'Disk image size' to 64GB or more if you use many images" -ForegroundColor White
Write-Host "• Disable 'Send usage statistics' and 'Send crash reports'" -ForegroundColor White
Write-Host "• Enable 'Check for updates automatically'" -ForegroundColor White

Write-Host "`n=== VERIFICATION STEPS ===" -ForegroundColor Cyan
Write-Host "After configuration, verify settings:" -ForegroundColor Yellow
Write-Host "1. Run: docker system info" -ForegroundColor White
Write-Host "2. Check memory and CPU limits in the output" -ForegroundColor White
Write-Host "3. Run: docker system df" -ForegroundColor White
Write-Host "4. Check disk usage and cleanup if needed" -ForegroundColor White

Write-Host "`n=== CONFIGURATION COMPLETE ===" -ForegroundColor Green
Write-Host "Docker Desktop configuration guide completed!" -ForegroundColor White
