# PowerShell script to expose local Docker API server to the internet using ngrok
# This allows the Render app to access your local Docker containers

Write-Host "🚀 Exposing Docker API Server to Internet..." -ForegroundColor Green

# Check if ngrok is installed
if (!(Get-Command ngrok -ErrorAction SilentlyContinue)) {
    Write-Host "❌ ngrok is not installed. Please install it first:" -ForegroundColor Red
    Write-Host "1. Download from: https://ngrok.com/download" -ForegroundColor Yellow
    Write-Host "2. Extract to a folder in your PATH" -ForegroundColor Yellow
    Write-Host "3. Sign up for a free account and get your auth token" -ForegroundColor Yellow
    Write-Host "4. Run: ngrok config add-authtoken YOUR_TOKEN" -ForegroundColor Yellow
    exit 1
}

# Check if Docker API server is running
$apiResponse = try { Invoke-WebRequest -Uri "http://localhost:5004/health" -TimeoutSec 5 } catch { $null }
if (!$apiResponse) {
    Write-Host "❌ Docker API server is not running on port 5004" -ForegroundColor Red
    Write-Host "Please start the Docker containers first:" -ForegroundColor Yellow
    Write-Host "docker-compose -f docker-compose.web.yml up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Docker API server is running on port 5004" -ForegroundColor Green

# Start ngrok tunnel
Write-Host "🌐 Starting ngrok tunnel..." -ForegroundColor Blue
Write-Host "This will expose your local Docker API server to the internet" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the tunnel" -ForegroundColor Yellow

# Start ngrok in background
$ngrokProcess = Start-Process -FilePath "ngrok" -ArgumentList "http", "5004" -PassThru -WindowStyle Hidden

# Wait a moment for ngrok to start
Start-Sleep -Seconds 3

# Get the public URL
try {
    $ngrokApi = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -TimeoutSec 5
    $publicUrl = $ngrokApi.tunnels[0].public_url
    
    if ($publicUrl) {
        Write-Host "🎉 Success! Your Docker API server is now accessible at:" -ForegroundColor Green
        Write-Host $publicUrl -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📋 Next steps:" -ForegroundColor Blue
        Write-Host "1. Update your Render app to use this URL" -ForegroundColor Yellow
        Write-Host "2. Or update the API_URL in your web app configuration" -ForegroundColor Yellow
        Write-Host "3. Keep this terminal open to maintain the tunnel" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "🔗 Test the API: $publicUrl/health" -ForegroundColor Cyan
        
        # Update the web app configuration
        $webConfig = Get-Content "Web/wwwroot/desktop-integrated.html" -Raw
        $updatedConfig = $webConfig -replace 'apiBaseUrl = null; // Use demo mode on Render', "apiBaseUrl = '$publicUrl/api'; // Use ngrok tunnel to Docker API"
        Set-Content "Web/wwwroot/desktop-integrated.html" -Value $updatedConfig
        
        Write-Host "✅ Updated web app configuration to use ngrok tunnel" -ForegroundColor Green
    } else {
        Write-Host "❌ Could not get ngrok public URL" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error getting ngrok tunnel info: $($_.Exception.Message)" -ForegroundColor Red
}

# Keep the script running
Write-Host "Press any key to stop the tunnel and exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Stop ngrok
if ($ngrokProcess -and !$ngrokProcess.HasExited) {
    $ngrokProcess.Kill()
    Write-Host "🛑 Stopped ngrok tunnel" -ForegroundColor Red
}
