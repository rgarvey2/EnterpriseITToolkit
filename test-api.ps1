Write-Host "Testing API Server..." -ForegroundColor Green

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5003/" -Method Get
    Write-Host "✅ API Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ API Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
}

Write-Host "`nTesting Health Endpoint..." -ForegroundColor Green

try {
    $health = Invoke-RestMethod -Uri "http://localhost:5003/health" -Method Get
    Write-Host "✅ Health Response:" -ForegroundColor Green
    $health | ConvertTo-Json
} catch {
    Write-Host "❌ Health Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTesting Login Endpoint..." -ForegroundColor Green

try {
    $loginData = @{
        username = "admin"
        password = "admin123"
    } | ConvertTo-Json

    $login = Invoke-RestMethod -Uri "http://localhost:5003/api/auth/login" -Method Post -Body $loginData -ContentType "application/json"
    Write-Host "✅ Login Response:" -ForegroundColor Green
    $login | ConvertTo-Json
} catch {
    Write-Host "❌ Login Error: $($_.Exception.Message)" -ForegroundColor Red
}
