# Deploy Enterprise IT Toolkit to Render
# This script builds the project and triggers a Render deployment

Write-Host "=== Enterprise IT Toolkit - Render Deployment ===" -ForegroundColor Yellow

# Step 1: Clean and build the project
Write-Host "`n1. Building project..." -ForegroundColor Cyan
Write-Host "   Cleaning previous build..." -ForegroundColor Gray
npm run clean

Write-Host "   Building with webpack..." -ForegroundColor Gray
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Please check the errors above." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build completed successfully!" -ForegroundColor Green

# Step 2: Verify build output
Write-Host "`n2. Verifying build output..." -ForegroundColor Cyan
$distPath = "Web\wwwroot\dist"
if (Test-Path $distPath) {
    $files = Get-ChildItem $distPath
    Write-Host "   Build files created:" -ForegroundColor Gray
    foreach ($file in $files) {
        $size = [math]::Round($file.Length / 1KB, 1)
        Write-Host "   • $($file.Name) ($size KB)" -ForegroundColor White
    }
    
    # Check if index.html exists
    if (Test-Path "$distPath\index.html") {
        Write-Host "✅ index.html found - ready for deployment!" -ForegroundColor Green
    } else {
        Write-Host "❌ index.html not found in dist folder!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ Dist folder not found!" -ForegroundColor Red
    exit 1
}

# Step 3: Commit and push changes
Write-Host "`n3. Committing and pushing changes..." -ForegroundColor Cyan

# Add all changes
git add .

# Check if there are changes to commit
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "   Committing changes..." -ForegroundColor Gray
    git commit -m "Deploy: Update build for Render deployment - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Git commit failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "   Pushing to GitHub..." -ForegroundColor Gray
    git push origin main
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Git push failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Changes pushed to GitHub successfully!" -ForegroundColor Green
} else {
    Write-Host "   No changes to commit - repository is up to date" -ForegroundColor Yellow
}

# Step 4: Deployment instructions
Write-Host "`n4. Render Deployment Instructions:" -ForegroundColor Cyan
Write-Host "   ✅ Build completed and pushed to GitHub" -ForegroundColor Green
Write-Host "   ✅ Render will automatically detect the changes" -ForegroundColor Green
Write-Host "   ✅ New deployment should start automatically" -ForegroundColor Green

Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Go to your Render dashboard" -ForegroundColor White
Write-Host "   2. Check the deployment status" -ForegroundColor White
Write-Host "   3. Wait for the build to complete" -ForegroundColor White
Write-Host "   4. Test the deployed application" -ForegroundColor White

Write-Host "`n🌐 Your Render service should be accessible at:" -ForegroundColor Cyan
Write-Host "   https://enterprise-toolkit-web.onrender.com" -ForegroundColor White

Write-Host "`n=== Deployment Process Complete ===" -ForegroundColor Yellow
Write-Host "The web interface should now work correctly with the fixed build process!" -ForegroundColor Green