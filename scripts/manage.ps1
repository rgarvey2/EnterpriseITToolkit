# Enterprise IT Toolkit Management Script (PowerShell)
# This script provides easy management commands for the production environment

param(
    [Parameter(Position=0)]
    [string]$Command = "help"
)

$ComposeFile = "docker-compose.production.yml"

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Header {
    param([string]$Message)
    Write-Host "================================" -ForegroundColor Blue
    Write-Host $Message -ForegroundColor Blue
    Write-Host "================================" -ForegroundColor Blue
}

# Function to check if Docker is running
function Test-Docker {
    try {
        docker info | Out-Null
        return $true
    }
    catch {
        Write-Error "Docker is not running. Please start Docker and try again."
        exit 1
    }
}

# Function to show help
function Show-Help {
    Write-Host "Enterprise IT Toolkit Management Script (PowerShell)"
    Write-Host ""
    Write-Host "Usage: .\scripts\manage.ps1 [COMMAND]"
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  start       Start all services"
    Write-Host "  stop        Stop all services"
    Write-Host "  restart     Restart all services"
    Write-Host "  status      Show service status"
    Write-Host "  logs        Show logs (all services)"
    Write-Host "  logs-api    Show API server logs"
    Write-Host "  logs-web    Show web server logs"
    Write-Host "  logs-db     Show database logs"
    Write-Host "  health      Check service health"
    Write-Host "  backup      Create manual backup"
    Write-Host "  update      Update and restart services"
    Write-Host "  shell-api   Open shell in API container"
    Write-Host "  shell-db    Open shell in database container"
    Write-Host "  ssl-gen     Generate self-signed SSL certificates"
    Write-Host "  clean       Clean up unused Docker resources"
    Write-Host "  help        Show this help message"
    Write-Host ""
}

# Main script logic
switch ($Command.ToLower()) {
    "start" {
        Write-Header "Starting Enterprise IT Toolkit"
        Test-Docker
        Write-Status "Starting all services..."
        docker-compose -f $ComposeFile up -d
        Write-Status "Services started successfully!"
    }
    
    "stop" {
        Write-Header "Stopping Enterprise IT Toolkit"
        Test-Docker
        Write-Status "Stopping all services..."
        docker-compose -f $ComposeFile down
        Write-Status "Services stopped successfully!"
    }
    
    "restart" {
        Write-Header "Restarting Enterprise IT Toolkit"
        Test-Docker
        Write-Status "Restarting all services..."
        docker-compose -f $ComposeFile restart
        Write-Status "Services restarted successfully!"
    }
    
    "status" {
        Write-Header "Service Status"
        Test-Docker
        docker-compose -f $ComposeFile ps
    }
    
    "logs" {
        Write-Header "Service Logs"
        Test-Docker
        docker-compose -f $ComposeFile logs -f
    }
    
    "logs-api" {
        Write-Header "API Server Logs"
        Test-Docker
        docker-compose -f $ComposeFile logs -f api-server
    }
    
    "logs-web" {
        Write-Header "Web Server Logs"
        Test-Docker
        docker-compose -f $ComposeFile logs -f web-server
    }
    
    "logs-db" {
        Write-Header "Database Logs"
        Test-Docker
        docker-compose -f $ComposeFile logs -f postgres
    }
    
    "health" {
        Write-Header "Health Check"
        Test-Docker
        
        Write-Status "Checking API server..."
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5004/health" -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                Write-Status "✅ API server is healthy"
            } else {
                Write-Error "❌ API server health check failed"
            }
        }
        catch {
            Write-Error "❌ API server health check failed"
        }
        
        Write-Status "Checking web server..."
        try {
            $response = Invoke-WebRequest -Uri "http://localhost/health" -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                Write-Status "✅ Web server is healthy"
            } else {
                Write-Error "❌ Web server health check failed"
            }
        }
        catch {
            Write-Error "❌ Web server health check failed"
        }
        
        Write-Status "Checking database..."
        try {
            docker-compose -f $ComposeFile exec postgres pg_isready -U enterprise_user -d enterprise_toolkit | Out-Null
            Write-Status "✅ Database is healthy"
        }
        catch {
            Write-Error "❌ Database health check failed"
        }
    }
    
    "backup" {
        Write-Header "Creating Manual Backup"
        Test-Docker
        Write-Status "Creating database backup..."
        docker-compose -f $ComposeFile exec backup /backup.sh
        Write-Status "Backup completed successfully!"
    }
    
    "update" {
        Write-Header "Updating Services"
        Test-Docker
        Write-Status "Pulling latest images..."
        docker-compose -f $ComposeFile pull
        Write-Status "Rebuilding and restarting services..."
        docker-compose -f $ComposeFile up -d --build
        Write-Status "Update completed successfully!"
    }
    
    "shell-api" {
        Write-Header "API Server Shell"
        Test-Docker
        docker-compose -f $ComposeFile exec api-server /bin/sh
    }
    
    "shell-db" {
        Write-Header "Database Shell"
        Test-Docker
        docker-compose -f $ComposeFile exec postgres psql -U enterprise_user -d enterprise_toolkit
    }
    
    "ssl-gen" {
        Write-Header "Generating SSL Certificates"
        if (Test-Path "scripts\generate-ssl.sh") {
            Write-Status "Note: SSL generation script is for Linux. For Windows, use OpenSSL or WSL."
            Write-Status "You can also use the existing certificates in nginx\ssl\"
        } else {
            Write-Error "SSL generation script not found"
        }
    }
    
    "clean" {
        Write-Header "Cleaning Up Docker Resources"
        Test-Docker
        $response = Read-Host "This will remove unused Docker resources. Continue? (y/N)"
        if ($response -match "^[yY]([eE][sS])?$") {
            docker system prune -f
            docker volume prune -f
            Write-Status "Cleanup completed!"
        } else {
            Write-Status "Cleanup cancelled."
        }
    }
    
    default {
        Show-Help
    }
}
