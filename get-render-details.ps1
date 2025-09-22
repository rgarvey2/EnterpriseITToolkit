# Get detailed Render service information
$env:RENDER_API_KEY = "rnd_S24Mnx5qXQsaDkJAWIM2scis1WFi"

Write-Host "=== Render Service Details ===" -ForegroundColor Yellow

try {
    $headers = @{
        "Authorization" = "Bearer $env:RENDER_API_KEY"
        "Content-Type" = "application/json"
    }
    
    # Get all services
    $services = Invoke-RestMethod -Uri "https://api.render.com/v1/services" -Method GET -Headers $headers
    
    Write-Host "`nFound $($services.Count) service(s):" -ForegroundColor Green
    
    foreach ($service in $services) {
        Write-Host "`n--- Service Details ---" -ForegroundColor Cyan
        Write-Host "ID: $($service.id)" -ForegroundColor White
        Write-Host "Name: $($service.name)" -ForegroundColor White
        Write-Host "Type: $($service.type)" -ForegroundColor White
        Write-Host "Owner: $($service.ownerId)" -ForegroundColor White
        
        if ($service.serviceDetails) {
            Write-Host "Environment: $($service.serviceDetails.env)" -ForegroundColor White
            Write-Host "Auto Deploy: $($service.serviceDetails.autoDeploy)" -ForegroundColor White
            Write-Host "URL: $($service.serviceDetails.url)" -ForegroundColor White
            Write-Host "Branch: $($service.serviceDetails.branch)" -ForegroundColor White
            Write-Host "Root Directory: $($service.serviceDetails.rootDir)" -ForegroundColor White
            Write-Host "Build Command: $($service.serviceDetails.buildCommand)" -ForegroundColor White
            Write-Host "Publish Path: $($service.serviceDetails.publishPath)" -ForegroundColor White
        }
        
        # Get deployments for this service
        try {
            $deployments = Invoke-RestMethod -Uri "https://api.render.com/v1/services/$($service.id)/deploys" -Method GET -Headers $headers
            Write-Host "Recent Deployments:" -ForegroundColor Yellow
            foreach ($deploy in $deployments | Select-Object -First 3) {
                $status = $deploy.deploy.status
                $color = switch ($status) {
                    "live" { "Green" }
                    "build_failed" { "Red" }
                    "update_failed" { "Red" }
                    "canceled" { "Yellow" }
                    default { "White" }
                }
                Write-Host "  • $($deploy.deploy.id) - $status - $($deploy.deploy.createdAt)" -ForegroundColor $color
            }
        }
        catch {
            Write-Host "  Could not fetch deployments: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Complete ===" -ForegroundColor Yellow
