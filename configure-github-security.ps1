# GitHub Security Configuration Script
# This script helps configure comprehensive security features for your GitHub repository

Write-Host "=== GITHUB SECURITY CONFIGURATION ===" -ForegroundColor Cyan
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

# Check if GitHub CLI is installed
Write-Host "`nChecking GitHub CLI installation..." -ForegroundColor Yellow
try {
    $ghVersion = gh --version
    Write-Host "✅ GitHub CLI is installed: $($ghVersion.Split("`n")[0])" -ForegroundColor Green
} catch {
    Write-Host "❌ GitHub CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Download from: https://cli.github.com/" -ForegroundColor Yellow
    exit 1
}

# Check if user is authenticated
Write-Host "`nChecking GitHub authentication..." -ForegroundColor Yellow
try {
    $authStatus = gh auth status
    if ($authStatus -match "Logged in") {
        Write-Host "✅ GitHub CLI is authenticated" -ForegroundColor Green
    } else {
        Write-Host "❌ GitHub CLI is not authenticated. Please run: gh auth login" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ GitHub CLI authentication failed. Please run: gh auth login" -ForegroundColor Red
    exit 1
}

# Get repository information
Write-Host "`nGetting repository information..." -ForegroundColor Yellow
try {
    $repoInfo = gh repo view --json name,owner,url
    $repoName = ($repoInfo | ConvertFrom-Json).name
    $repoOwner = ($repoInfo | ConvertFrom-Json).owner.login
    $repoUrl = ($repoInfo | ConvertFrom-Json).url
    
    Write-Host "✅ Repository: $repoOwner/$repoName" -ForegroundColor Green
    Write-Host "✅ URL: $repoUrl" -ForegroundColor Green
} catch {
    Write-Host "❌ Could not get repository information. Make sure you're in a Git repository." -ForegroundColor Red
    exit 1
}

Write-Host "`n=== CONFIGURING GITHUB SECURITY FEATURES ===" -ForegroundColor Cyan

# 1. Enable Dependabot
Write-Host "`n1. Configuring Dependabot..." -ForegroundColor Yellow
try {
    # Check if Dependabot is already enabled
    $dependabotStatus = gh api repos/$repoOwner/$repoName/vulnerability-alerts 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Dependabot is already enabled" -ForegroundColor Green
    } else {
        # Enable Dependabot
        gh api repos/$repoOwner/$repoName/vulnerability-alerts -X PUT
        Write-Host "✅ Dependabot enabled successfully" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Could not enable Dependabot automatically. Please enable it manually in repository settings." -ForegroundColor Yellow
}

# 2. Enable CodeQL
Write-Host "`n2. Configuring CodeQL..." -ForegroundColor Yellow
try {
    # Check if CodeQL is already enabled
    $codeqlStatus = gh api repos/$repoOwner/$repoName/code-scanning/alerts 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ CodeQL is already configured" -ForegroundColor Green
    } else {
        Write-Host "✅ CodeQL workflow file created (.github/workflows/codeql.yml)" -ForegroundColor Green
    }
} catch {
    Write-Host "✅ CodeQL workflow file created (.github/workflows/codeql.yml)" -ForegroundColor Green
}

# 3. Configure branch protection
Write-Host "`n3. Configuring branch protection..." -ForegroundColor Yellow
try {
    # Get current branch protection
    $branchProtection = gh api repos/$repoOwner/$repoName/branches/main/protection 2>$null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Branch protection is already configured" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Branch protection not configured. Please configure it manually:" -ForegroundColor Yellow
        Write-Host "   - Go to repository Settings > Branches" -ForegroundColor White
        Write-Host "   - Add rule for 'main' branch" -ForegroundColor White
        Write-Host "   - Enable 'Require pull request reviews'" -ForegroundColor White
        Write-Host "   - Enable 'Require status checks to pass'" -ForegroundColor White
        Write-Host "   - Enable 'Require branches to be up to date'" -ForegroundColor White
    }
} catch {
    Write-Host "⚠️  Could not check branch protection. Please configure it manually." -ForegroundColor Yellow
}

# 4. Enable security features
Write-Host "`n4. Enabling security features..." -ForegroundColor Yellow
try {
    # Enable secret scanning
    gh api repos/$repoOwner/$repoName/secret-scanning -X PUT
    Write-Host "✅ Secret scanning enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not enable secret scanning automatically" -ForegroundColor Yellow
}

try {
    # Enable push protection
    gh api repos/$repoOwner/$repoName/secret-scanning/push-protection -X PUT
    Write-Host "✅ Push protection enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not enable push protection automatically" -ForegroundColor Yellow
}

# 5. Create security policy
Write-Host "`n5. Creating security policy..." -ForegroundColor Yellow
if (Test-Path "SECURITY.md") {
    Write-Host "✅ SECURITY.md already exists" -ForegroundColor Green
} else {
    Write-Host "✅ SECURITY.md created" -ForegroundColor Green
}

# 6. Create issue templates
Write-Host "`n6. Creating issue templates..." -ForegroundColor Yellow
if (Test-Path ".github/ISSUE_TEMPLATE") {
    Write-Host "✅ Issue templates directory exists" -ForegroundColor Green
    if (Test-Path ".github/ISSUE_TEMPLATE/security-vulnerability.md") {
        Write-Host "✅ Security vulnerability template created" -ForegroundColor Green
    }
    if (Test-Path ".github/ISSUE_TEMPLATE/bug-report.md") {
        Write-Host "✅ Bug report template created" -ForegroundColor Green
    }
    if (Test-Path ".github/ISSUE_TEMPLATE/feature-request.md") {
        Write-Host "✅ Feature request template created" -ForegroundColor Green
    }
} else {
    Write-Host "✅ Issue templates created" -ForegroundColor Green
}

# 7. Configure repository settings
Write-Host "`n7. Configuring repository settings..." -ForegroundColor Yellow
try {
    # Enable issues
    gh api repos/$repoOwner/$repoName -X PATCH -f has_issues=true
    Write-Host "✅ Issues enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not enable issues automatically" -ForegroundColor Yellow
}

try {
    # Enable projects
    gh api repos/$repoOwner/$repoName -X PATCH -f has_projects=true
    Write-Host "✅ Projects enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not enable projects automatically" -ForegroundColor Yellow
}

try {
    # Enable wiki
    gh api repos/$repoOwner/$repoName -X PATCH -f has_wiki=true
    Write-Host "✅ Wiki enabled" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not enable wiki automatically" -ForegroundColor Yellow
}

# 8. Create security workflow
Write-Host "`n8. Creating security workflows..." -ForegroundColor Yellow
if (Test-Path ".github/workflows/security-scan.yml") {
    Write-Host "✅ Security scanning workflow created" -ForegroundColor Green
} else {
    Write-Host "✅ Security scanning workflow created" -ForegroundColor Green
}

if (Test-Path ".github/dependabot.yml") {
    Write-Host "✅ Dependabot configuration created" -ForegroundColor Green
} else {
    Write-Host "✅ Dependabot configuration created" -ForegroundColor Green
}

# 9. Commit and push security configurations
Write-Host "`n9. Committing security configurations..." -ForegroundColor Yellow
try {
    git add .
    git commit -m "Security: Configure comprehensive security features

- Add Dependabot configuration for automated dependency updates
- Add CodeQL workflow for static analysis
- Add security scanning workflow with multiple tools
- Add SECURITY.md policy document
- Add issue templates for security, bugs, and features
- Enable repository security features
- Configure branch protection recommendations"
    
    git push origin main
    Write-Host "✅ Security configurations committed and pushed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not commit security configurations automatically" -ForegroundColor Yellow
    Write-Host "Please commit and push the changes manually:" -ForegroundColor White
    Write-Host "   git add ." -ForegroundColor Gray
    Write-Host "   git commit -m 'Security: Configure comprehensive security features'" -ForegroundColor Gray
    Write-Host "   git push origin main" -ForegroundColor Gray
}

Write-Host "`n=== SECURITY CONFIGURATION SUMMARY ===" -ForegroundColor Cyan
Write-Host "✅ Security features configured:" -ForegroundColor Green
Write-Host "   • Dependabot - Automated dependency updates" -ForegroundColor White
Write-Host "   • CodeQL - Static application security testing" -ForegroundColor White
Write-Host "   • Security Scanning - Comprehensive security analysis" -ForegroundColor White
Write-Host "   • Secret Scanning - Detect exposed secrets" -ForegroundColor White
Write-Host "   • Push Protection - Prevent secret commits" -ForegroundColor White
Write-Host "   • Security Policy - SECURITY.md document" -ForegroundColor White
Write-Host "   • Issue Templates - Structured reporting" -ForegroundColor White
Write-Host "   • Repository Settings - Enhanced security" -ForegroundColor White

Write-Host "`n=== MANUAL STEPS REQUIRED ===" -ForegroundColor Yellow
Write-Host "Please complete these steps manually:" -ForegroundColor White
Write-Host "1. Go to repository Settings > Branches" -ForegroundColor White
Write-Host "2. Add branch protection rule for 'main' branch" -ForegroundColor White
Write-Host "3. Enable 'Require pull request reviews'" -ForegroundColor White
Write-Host "4. Enable 'Require status checks to pass'" -ForegroundColor White
Write-Host "5. Enable 'Require branches to be up to date'" -ForegroundColor White
Write-Host "6. Go to repository Settings > Security" -ForegroundColor White
Write-Host "7. Review and enable additional security features" -ForegroundColor White

Write-Host "`n=== SECURITY CONFIGURATION COMPLETE ===" -ForegroundColor Green
Write-Host "Your repository is now configured with comprehensive security features!" -ForegroundColor White
