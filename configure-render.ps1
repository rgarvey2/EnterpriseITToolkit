# Render Configuration and Deployment Script
# This script configures Render API access and provides deployment management

param(
    [string]$Action = "configure",
    [string]$ServiceName = "enterprise-toolkit-web"
)

# Set Render API Key
$env:RENDER_API_KEY = "rnd_S24Mnx5qXQsaDkJAWIM2scis1WFi"

Write-Host "=== Render Configuration Script ===" -ForegroundColor Yellow
Write-Host "API Key configured: $($env:RENDER_API_KEY.Substring(0, 10))..." -ForegroundColor Green

function Test-RenderAPI {
    Write-Host "`nTesting Render API connection..." -ForegroundColor Cyan
    
    try {
        $headers = @{
            "Authorization" = "Bearer $env:RENDER_API_KEY"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "https://api.render.com/v1/services" -Method GET -Headers $headers
        Write-Host "✅ Render API connection successful!" -ForegroundColor Green
        Write-Host "Found $($response.Count) services" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Render API connection failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Get-RenderServices {
    Write-Host "`nFetching Render services..." -ForegroundColor Cyan
    
    try {
        $headers = @{
            "Authorization" = "Bearer $env:RENDER_API_KEY"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "https://api.render.com/v1/services" -Method GET -Headers $headers
        
        Write-Host "`n=== Render Services ===" -ForegroundColor Yellow
        foreach ($service in $response) {
            $status = if ($service.serviceDetails) { $service.serviceDetails.env } else { "Unknown" }
            Write-Host "• $($service.name) - $($service.type) - $status" -ForegroundColor White
            if ($service.serviceDetails -and $service.serviceDetails.url) {
                Write-Host "  URL: $($service.serviceDetails.url)" -ForegroundColor Gray
            }
        }
        
        return $response
    }
    catch {
        Write-Host "❌ Failed to fetch services: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

function Deploy-ToRender {
    Write-Host "`n=== Deploying to Render ===" -ForegroundColor Yellow
    
    # Build the project first
    Write-Host "1. Building project..." -ForegroundColor Cyan
    npm run clean
    npm run build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    
    # Commit and push changes
    Write-Host "2. Committing and pushing changes..." -ForegroundColor Cyan
    git add .
    git commit -m "Deploy: Update build for Render deployment - $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    git push origin main
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Git push failed!" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✅ Changes pushed to GitHub!" -ForegroundColor Green
    Write-Host "3. Render will automatically deploy from GitHub..." -ForegroundColor Cyan
    Write-Host "   Check your Render dashboard for deployment status" -ForegroundColor Gray
    
    return $true
}

function Get-DeploymentStatus {
    Write-Host "`n=== Deployment Status ===" -ForegroundColor Yellow
    
    $services = Get-RenderServices
    if ($services) {
        $targetService = $services | Where-Object { $_.name -like "*$ServiceName*" }
        
        if ($targetService) {
            Write-Host "✅ Found service: $($targetService.name)" -ForegroundColor Green
            
            if ($targetService.serviceDetails -and $targetService.serviceDetails.url) {
                Write-Host "🌐 Service URL: $($targetService.serviceDetails.url)" -ForegroundColor Cyan
                
                # Test if service is accessible
                try {
                    $response = Invoke-WebRequest -Uri $targetService.serviceDetails.url -Method GET -TimeoutSec 10
                    if ($response.StatusCode -eq 200) {
                        Write-Host "✅ Service is accessible and responding!" -ForegroundColor Green
                    } else {
                        Write-Host "⚠️ Service responded with status: $($response.StatusCode)" -ForegroundColor Yellow
                    }
                }
                catch {
                    Write-Host "❌ Service is not accessible: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        } else {
            Write-Host "❌ Service '$ServiceName' not found" -ForegroundColor Red
        }
    }
}

# Main execution
switch ($Action.ToLower()) {
    "configure" {
        Write-Host "Configuring Render API access..." -ForegroundColor Cyan
        Test-RenderAPI
    }
    "test" {
        Test-RenderAPI
        Get-RenderServices
    }
    "deploy" {
        Deploy-ToRender
    }
    "status" {
        Get-DeploymentStatus
    }
    "services" {
        Get-RenderServices
    }
    default {
        Write-Host "Available actions:" -ForegroundColor Yellow
        Write-Host "  configure - Set up Render API access" -ForegroundColor White
        Write-Host "  test      - Test API connection and list services" -ForegroundColor White
        Write-Host "  deploy    - Build and deploy to Render" -ForegroundColor White
        Write-Host "  status    - Check deployment status" -ForegroundColor White
        Write-Host "  services  - List all Render services" -ForegroundColor White
        Write-Host "`nUsage: .\configure-render.ps1 -Action [action]" -ForegroundColor Cyan
    }
}

Write-Host "`n=== Script Complete ===" -ForegroundColor Yellow
