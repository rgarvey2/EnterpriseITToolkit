# Virtual Memory Configuration Script
# This script helps configure virtual memory settings for optimal performance

Write-Host "=== VIRTUAL MEMORY CONFIGURATION ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Get system information
$totalMemory = [math]::Round((Get-WmiObject -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
Write-Host "`nSystem Information:" -ForegroundColor Yellow
Write-Host "Total Physical Memory: $totalMemory GB" -ForegroundColor White

# Calculate recommended virtual memory settings
$initialSize = [math]::Round($totalMemory * 1.5, 0)  # 1.5x physical memory
$maximumSize = [math]::Round($totalMemory * 3, 0)   # 3x physical memory

Write-Host "`nRecommended Virtual Memory Settings:" -ForegroundColor Yellow
Write-Host "Initial Size: $initialSize MB" -ForegroundColor Green
Write-Host "Maximum Size: $maximumSize MB" -ForegroundColor Green

# Get current virtual memory settings
Write-Host "`nCurrent Virtual Memory Settings:" -ForegroundColor Yellow
try {
    $pageFile = Get-WmiObject -Class Win32_PageFileSetting
    if ($pageFile) {
        foreach ($file in $pageFile) {
            Write-Host "Drive: $($file.Name)" -ForegroundColor White
            Write-Host "Initial Size: $($file.InitialSize) MB" -ForegroundColor White
            Write-Host "Maximum Size: $($file.MaximumSize) MB" -ForegroundColor White
        }
    } else {
        Write-Host "No page file settings found" -ForegroundColor Red
    }
} catch {
    Write-Host "Could not retrieve current settings: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== MANUAL CONFIGURATION STEPS ===" -ForegroundColor Cyan
Write-Host "To configure virtual memory manually:" -ForegroundColor Yellow
Write-Host "1. Press Windows + R, type 'sysdm.cpl' and press Enter" -ForegroundColor White
Write-Host "2. Click 'Advanced' tab" -ForegroundColor White
Write-Host "3. Under 'Performance', click 'Settings'" -ForegroundColor White
Write-Host "4. Click 'Advanced' tab" -ForegroundColor White
Write-Host "5. Under 'Virtual memory', click 'Change'" -ForegroundColor White
Write-Host "6. Uncheck 'Automatically manage paging file size for all drives'" -ForegroundColor White
Write-Host "7. Select your system drive (usually C:)" -ForegroundColor White
Write-Host "8. Select 'Custom size'" -ForegroundColor White
Write-Host "9. Set Initial size: $initialSize MB" -ForegroundColor Green
Write-Host "10. Set Maximum size: $maximumSize MB" -ForegroundColor Green
Write-Host "11. Click 'Set', then 'OK'" -ForegroundColor White
Write-Host "12. Restart your computer when prompted" -ForegroundColor White

Write-Host "`n=== AUTOMATED CONFIGURATION (ADVANCED) ===" -ForegroundColor Cyan
Write-Host "WARNING: This will modify system settings. Use with caution!" -ForegroundColor Red
$confirm = Read-Host "Do you want to attempt automated configuration? (y/N)"

if ($confirm -eq 'y' -or $confirm -eq 'Y') {
    Write-Host "`nAttempting automated configuration..." -ForegroundColor Yellow
    
    try {
        # This requires administrative privileges
        if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
            Write-Host "ERROR: This script must be run as Administrator for automated configuration" -ForegroundColor Red
            Write-Host "Please run PowerShell as Administrator and try again" -ForegroundColor Yellow
            exit 1
        }
        
        # Get system drive
        $systemDrive = $env:SystemDrive
        Write-Host "Configuring virtual memory on drive: $systemDrive" -ForegroundColor White
        
        # Configure virtual memory using WMI
        $pageFile = Get-WmiObject -Class Win32_PageFileSetting | Where-Object { $_.Name -like "$systemDrive*" }
        
        if ($pageFile) {
            $pageFile.InitialSize = $initialSize
            $pageFile.MaximumSize = $maximumSize
            $result = $pageFile.Put()
            
            if ($result.ReturnValue -eq 0) {
                Write-Host "✅ Virtual memory configured successfully!" -ForegroundColor Green
                Write-Host "Initial Size: $initialSize MB" -ForegroundColor Green
                Write-Host "Maximum Size: $maximumSize MB" -ForegroundColor Green
                Write-Host "`n⚠️  RESTART REQUIRED: Please restart your computer for changes to take effect" -ForegroundColor Yellow
            } else {
                Write-Host "❌ Failed to configure virtual memory. Return code: $($result.ReturnValue)" -ForegroundColor Red
            }
        } else {
            Write-Host "❌ Could not find page file settings for system drive" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ Error during automated configuration: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Please use manual configuration steps above" -ForegroundColor Yellow
    }
} else {
    Write-Host "`nUsing manual configuration. Please follow the steps above." -ForegroundColor Yellow
}

Write-Host "`n=== CONFIGURATION COMPLETE ===" -ForegroundColor Green
Write-Host "Virtual memory configuration guide completed!" -ForegroundColor White
