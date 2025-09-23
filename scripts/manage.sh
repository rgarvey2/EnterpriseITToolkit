#!/bin/bash

# Enterprise IT Toolkit Management Script
# This script provides easy management commands for the production environment

set -e

COMPOSE_FILE="docker-compose.production.yml"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}================================${NC}"
}

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
}

# Function to show help
show_help() {
    echo "Enterprise IT Toolkit Management Script"
    echo ""
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  start       Start all services"
    echo "  stop        Stop all services"
    echo "  restart     Restart all services"
    echo "  status      Show service status"
    echo "  logs        Show logs (all services)"
    echo "  logs-api    Show API server logs"
    echo "  logs-web    Show web server logs"
    echo "  logs-db     Show database logs"
    echo "  health      Check service health"
    echo "  backup      Create manual backup"
    echo "  update      Update and restart services"
    echo "  shell-api   Open shell in API container"
    echo "  shell-db    Open shell in database container"
    echo "  ssl-gen     Generate self-signed SSL certificates"
    echo "  ssl-renew   Renew Let's Encrypt certificates"
    echo "  clean       Clean up unused Docker resources"
    echo "  help        Show this help message"
    echo ""
}

# Main script logic
case "${1:-help}" in
    start)
        print_header "Starting Enterprise IT Toolkit"
        check_docker
        print_status "Starting all services..."
        docker-compose -f $COMPOSE_FILE up -d
        print_status "Services started successfully!"
        ;;
    
    stop)
        print_header "Stopping Enterprise IT Toolkit"
        check_docker
        print_status "Stopping all services..."
        docker-compose -f $COMPOSE_FILE down
        print_status "Services stopped successfully!"
        ;;
    
    restart)
        print_header "Restarting Enterprise IT Toolkit"
        check_docker
        print_status "Restarting all services..."
        docker-compose -f $COMPOSE_FILE restart
        print_status "Services restarted successfully!"
        ;;
    
    status)
        print_header "Service Status"
        check_docker
        docker-compose -f $COMPOSE_FILE ps
        ;;
    
    logs)
        print_header "Service Logs"
        check_docker
        docker-compose -f $COMPOSE_FILE logs -f
        ;;
    
    logs-api)
        print_header "API Server Logs"
        check_docker
        docker-compose -f $COMPOSE_FILE logs -f api-server
        ;;
    
    logs-web)
        print_header "Web Server Logs"
        check_docker
        docker-compose -f $COMPOSE_FILE logs -f web-server
        ;;
    
    logs-db)
        print_header "Database Logs"
        check_docker
        docker-compose -f $COMPOSE_FILE logs -f postgres
        ;;
    
    health)
        print_header "Health Check"
        check_docker
        
        print_status "Checking API server..."
        if curl -f http://localhost:5004/health > /dev/null 2>&1; then
            print_status "✅ API server is healthy"
        else
            print_error "❌ API server health check failed"
        fi
        
        print_status "Checking web server..."
        if curl -f http://localhost/health > /dev/null 2>&1; then
            print_status "✅ Web server is healthy"
        else
            print_error "❌ Web server health check failed"
        fi
        
        print_status "Checking database..."
        if docker-compose -f $COMPOSE_FILE exec postgres pg_isready -U enterprise_user -d enterprise_toolkit > /dev/null 2>&1; then
            print_status "✅ Database is healthy"
        else
            print_error "❌ Database health check failed"
        fi
        ;;
    
    backup)
        print_header "Creating Manual Backup"
        check_docker
        print_status "Creating database backup..."
        docker-compose -f $COMPOSE_FILE exec backup /backup.sh
        print_status "Backup completed successfully!"
        ;;
    
    update)
        print_header "Updating Services"
        check_docker
        print_status "Pulling latest images..."
        docker-compose -f $COMPOSE_FILE pull
        print_status "Rebuilding and restarting services..."
        docker-compose -f $COMPOSE_FILE up -d --build
        print_status "Update completed successfully!"
        ;;
    
    shell-api)
        print_header "API Server Shell"
        check_docker
        docker-compose -f $COMPOSE_FILE exec api-server /bin/sh
        ;;
    
    shell-db)
        print_header "Database Shell"
        check_docker
        docker-compose -f $COMPOSE_FILE exec postgres psql -U enterprise_user -d enterprise_toolkit
        ;;
    
    ssl-gen)
        print_header "Generating SSL Certificates"
        if [ -f "scripts/generate-ssl.sh" ]; then
            chmod +x scripts/generate-ssl.sh
            ./scripts/generate-ssl.sh localhost
        else
            print_error "SSL generation script not found"
        fi
        ;;
    
    ssl-renew)
        print_header "Renewing SSL Certificates"
        if [ -f "scripts/renew-ssl.sh" ]; then
            chmod +x scripts/renew-ssl.sh
            ./scripts/renew-ssl.sh
        else
            print_error "SSL renewal script not found"
        fi
        ;;
    
    clean)
        print_header "Cleaning Up Docker Resources"
        check_docker
        print_warning "This will remove unused Docker resources. Continue? (y/N)"
        read -r response
        if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
            docker system prune -f
            docker volume prune -f
            print_status "Cleanup completed!"
        else
            print_status "Cleanup cancelled."
        fi
        ;;
    
    help|*)
        show_help
        ;;
esac
