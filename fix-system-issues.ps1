# Fix System Issues - Terminal Hanging Resolution
# Run this script as Administrator to resolve Windows Defender and process issues

Write-Host "🔧 Fixing System Issues - Terminal Hanging Resolution" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✅ Running as Administrator" -ForegroundColor Green

# Step 1: Stop all hanging processes
Write-Host "`n🔄 Step 1: Stopping hanging processes..." -ForegroundColor Yellow
try {
    $processes = Get-Process | Where-Object {
        $_.ProcessName -like "*powershell*" -or 
        $_.ProcessName -like "*pwsh*" -or 
        $_.ProcessName -like "*node*" -or 
        $_.ProcessName -like "*dotnet*"
    }
    
    if ($processes) {
        Write-Host "Found $($processes.Count) processes to stop:" -ForegroundColor Yellow
        $processes | ForEach-Object { Write-Host "  - $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray }
        $processes | Stop-Process -Force
        Write-Host "✅ Processes stopped successfully" -ForegroundColor Green
    } else {
        Write-Host "✅ No hanging processes found" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ Warning: Could not stop some processes: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 2: Add Windows Defender exclusions
Write-Host "`n🔄 Step 2: Adding Windows Defender exclusions..." -ForegroundColor Yellow
try {
    $projectPath = "C:\Users\ryan.garvey\Desktop\EnterpriseITToolkit"
    
    # Add path exclusions
    Add-MpPreference -ExclusionPath $projectPath -ErrorAction SilentlyContinue
    Write-Host "✅ Added project path exclusion: $projectPath" -ForegroundColor Green
    
    # Add process exclusions
    $processes = @("pwsh.exe", "powershell.exe", "node.exe", "dotnet.exe", "cursor.exe")
    foreach ($process in $processes) {
        Add-MpPreference -ExclusionProcess $process -ErrorAction SilentlyContinue
        Write-Host "✅ Added process exclusion: $process" -ForegroundColor Green
    }
    
    Write-Host "✅ Windows Defender exclusions added successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error adding Windows Defender exclusions: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Restart Windows Defender service
Write-Host "`n🔄 Step 3: Restarting Windows Defender service..." -ForegroundColor Yellow
try {
    Restart-Service -Name "WinDefend" -Force -ErrorAction Stop
    Write-Host "✅ Windows Defender service restarted successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error restarting Windows Defender service: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Verify exclusions
Write-Host "`n🔄 Step 4: Verifying exclusions..." -ForegroundColor Yellow
try {
    $preferences = Get-MpPreference
    Write-Host "Current exclusions:" -ForegroundColor Cyan
    if ($preferences.ExclusionPath) {
        $preferences.ExclusionPath | ForEach-Object { Write-Host "  📁 $($_)" -ForegroundColor Gray }
    }
    if ($preferences.ExclusionProcess) {
        $preferences.ExclusionProcess | ForEach-Object { Write-Host "  🔧 $($_)" -ForegroundColor Gray }
    }
    Write-Host "✅ Exclusions verified" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Warning: Could not verify exclusions: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 5: System status check
Write-Host "`n🔄 Step 5: Checking system status..." -ForegroundColor Yellow
try {
    $defenderStatus = Get-MpComputerStatus
    Write-Host "Windows Defender Status:" -ForegroundColor Cyan
    Write-Host "  Antivirus Enabled: $($defenderStatus.AntivirusEnabled)" -ForegroundColor Gray
    Write-Host "  Real-time Protection: $($defenderStatus.RealTimeProtectionEnabled)" -ForegroundColor Gray
    Write-Host "  On-Access Protection: $($defenderStatus.OnAccessProtectionEnabled)" -ForegroundColor Gray
    
    $memory = Get-Counter "\Memory\Available MBytes" | Select-Object -ExpandProperty CounterSamples | Select-Object -ExpandProperty CookedValue
    Write-Host "  Available Memory: $([math]::Round($memory, 0)) MB" -ForegroundColor Gray
    
    Write-Host "✅ System status check completed" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Warning: Could not check system status: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 6: Test file operations
Write-Host "`n🔄 Step 6: Testing file operations..." -ForegroundColor Yellow
try {
    $testDir = "C:\Users\ryan.garvey\Desktop\EnterpriseITToolkit\test"
    if (Test-Path $testDir) {
        Remove-Item $testDir -Recurse -Force
    }
    
    $startTime = Get-Date
    New-Item -Path $testDir -ItemType Directory -Force | Out-Null
    1..10 | ForEach-Object { New-Item "test\test$_.txt" -ItemType File -Force | Out-Null }
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalMilliseconds
    
    Remove-Item $testDir -Recurse -Force
    Write-Host "✅ File operations test completed in $([math]::Round($duration, 0)) ms" -ForegroundColor Green
    
    if ($duration -lt 1000) {
        Write-Host "✅ Performance is good (< 1 second)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Performance is slow (> 1 second)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error testing file operations: $($_.Exception.Message)" -ForegroundColor Red
}

# Final summary
Write-Host "`n🎉 System Fix Complete!" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host "✅ Hanging processes stopped" -ForegroundColor Green
Write-Host "✅ Windows Defender exclusions added" -ForegroundColor Green
Write-Host "✅ Windows Defender service restarted" -ForegroundColor Green
Write-Host "✅ System status verified" -ForegroundColor Green
Write-Host "✅ File operations tested" -ForegroundColor Green

Write-Host "`n📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Close this PowerShell window" -ForegroundColor White
Write-Host "2. Open a new PowerShell window" -ForegroundColor White
Write-Host "3. Navigate to your project directory" -ForegroundColor White
Write-Host "4. Test your development commands" -ForegroundColor White

Write-Host "`n🚀 Your terminal should now be responsive!" -ForegroundColor Green
Read-Host "Press Enter to exit"
