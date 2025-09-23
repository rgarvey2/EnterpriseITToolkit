#!/bin/bash

# Production Deployment Script
# This script deploys the Enterprise IT Toolkit to production

set -e

echo "🚀 Enterprise IT Toolkit - Production Deployment"
echo "================================================"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if production environment file exists
if [ ! -f "production.env" ]; then
    echo "❌ Error: production.env file not found."
    echo "Please copy production.env.example to production.env and configure it."
    exit 1
fi

# Load environment variables
echo "📋 Loading production environment..."
export $(cat production.env | grep -v '^#' | xargs)

# Generate SSL certificates if they don't exist
if [ ! -f "nginx/ssl/cert.pem" ] || [ ! -f "nginx/ssl/key.pem" ]; then
    echo "🔐 SSL certificates not found. Generating self-signed certificates..."
    chmod +x scripts/generate-ssl.sh
    ./scripts/generate-ssl.sh localhost
fi

# Create necessary directories
echo "📁 Creating necessary directories..."
mkdir -p logs/nginx logs/api logs/postgres logs/redis
mkdir -p data backups
mkdir -p grafana/provisioning grafana/dashboards
mkdir -p prometheus

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker-compose -f docker-compose.production.yml down || true

# Build and start production containers
echo "🔨 Building and starting production containers..."
docker-compose -f docker-compose.production.yml up -d --build

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
sleep 30

# Check service health
echo "🏥 Checking service health..."
docker-compose -f docker-compose.production.yml ps

# Test API endpoint
echo "🧪 Testing API endpoint..."
if curl -f http://localhost:5004/health > /dev/null 2>&1; then
    echo "✅ API server is healthy"
else
    echo "❌ API server health check failed"
fi

# Test web server
echo "🧪 Testing web server..."
if curl -f http://localhost/health > /dev/null 2>&1; then
    echo "✅ Web server is healthy"
else
    echo "❌ Web server health check failed"
fi

echo ""
echo "🎉 Production deployment completed!"
echo "=================================="
echo ""
echo "🌐 Access URLs:"
echo "   • Web Application: https://localhost (or your domain)"
echo "   • API Server: http://localhost:5004"
echo "   • Admin Panel: https://localhost/admin"
echo "   • Grafana: https://localhost/grafana"
echo "   • Prometheus: https://localhost/prometheus"
echo ""
echo "🔐 Default Credentials:"
echo "   • Admin Panel: admin / admin123"
echo "   • Grafana: admin / $GRAFANA_PASSWORD"
echo ""
echo "📊 Monitoring:"
echo "   • View logs: docker-compose -f docker-compose.production.yml logs -f"
echo "   • Check status: docker-compose -f docker-compose.production.yml ps"
echo ""
echo "🔄 Management Commands:"
echo "   • Stop: docker-compose -f docker-compose.production.yml down"
echo "   • Restart: docker-compose -f docker-compose.production.yml restart"
echo "   • Update: docker-compose -f docker-compose.production.yml pull && docker-compose -f docker-compose.production.yml up -d"
echo ""
echo "⚠️  Security Notes:"
echo "   • Change all default passwords in production.env"
echo "   • Use Let's Encrypt for SSL certificates in production"
echo "   • Configure firewall rules"
echo "   • Enable monitoring and alerting"
