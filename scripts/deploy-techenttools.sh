#!/bin/bash

# Enterprise IT Toolkit - TechEntTools.com Deployment Script
# This script deploys the Enterprise IT Toolkit to app.techenttools.com

set -e

echo "🚀 Enterprise IT Toolkit - TechEntTools.com Deployment"
echo "======================================================"
echo "🌐 Domain: app.techenttools.com"
echo "📧 Email: admin@techenttools.com"
echo ""

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

# Generate SSL certificates for app.techenttools.com
echo "🔐 Setting up SSL certificates for app.techenttools.com..."

# Check if Let's Encrypt certificates exist
if [ -f "/etc/letsencrypt/live/app.techenttools.com/fullchain.pem" ]; then
    echo "✅ Let's Encrypt certificates found, copying to nginx/ssl/"
    sudo cp "/etc/letsencrypt/live/app.techenttools.com/fullchain.pem" ./nginx/ssl/cert.pem
    sudo cp "/etc/letsencrypt/live/app.techenttools.com/privkey.pem" ./nginx/ssl/key.pem
    sudo chown $(whoami):$(whoami) ./nginx/ssl/cert.pem ./nginx/ssl/key.pem
    chmod 644 ./nginx/ssl/cert.pem
    chmod 600 ./nginx/ssl/key.pem
else
    echo "⚠️  Let's Encrypt certificates not found."
    echo "🔐 Generating self-signed certificates for app.techenttools.com..."
    chmod +x scripts/generate-ssl.sh
    ./scripts/generate-ssl.sh app.techenttools.com
    echo ""
    echo "⚠️  IMPORTANT: For production, set up Let's Encrypt certificates:"
    echo "   ./scripts/setup-letsencrypt.sh app.techenttools.com admin@techenttools.com"
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
echo "🎉 TechEntTools.com deployment completed!"
echo "========================================"
echo ""
echo "🌐 Access URLs:"
echo "   • Web Application: https://app.techenttools.com"
echo "   • API Server: http://app.techenttools.com:5004"
echo "   • Admin Panel: https://app.techenttools.com/admin"
echo "   • Grafana: https://app.techenttools.com/grafana"
echo "   • Prometheus: https://app.techenttools.com/prometheus"
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
echo "⚠️  Next Steps:"
echo "   1. Configure DNS: Point app.techenttools.com to this server's IP"
echo "   2. Set up Let's Encrypt: ./scripts/setup-letsencrypt.sh app.techenttools.com admin@techenttools.com"
echo "   3. Change default passwords in production.env"
echo "   4. Configure firewall rules"
echo "   5. Set up monitoring and alerting"
echo ""
echo "🔧 DNS Configuration Required:"
echo "   A Record: app.techenttools.com → $(curl -s ifconfig.me)"
echo "   CNAME: www.app.techenttools.com → app.techenttools.com"
