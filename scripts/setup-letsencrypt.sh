#!/bin/bash

# Let's Encrypt SSL Certificate Setup Script
# This script sets up Let's Encrypt certificates for production

set -e

DOMAIN=${1}
EMAIL=${2:-admin@$DOMAIN}

if [ -z "$DOMAIN" ]; then
    echo "❌ Error: Domain is required"
    echo "Usage: $0 <domain> [email]"
    echo "Example: $0 yourdomain.com admin@yourdomain.com"
    exit 1
fi

echo "🔐 Setting up Let's Encrypt SSL certificates for domain: $DOMAIN"
echo "📧 Email: $EMAIL"

# Create SSL directory
mkdir -p ./nginx/ssl

# Install certbot if not already installed
if ! command -v certbot &> /dev/null; then
    echo "📦 Installing certbot..."
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        sudo apt-get update
        sudo apt-get install -y certbot
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        brew install certbot
    else
        echo "❌ Error: Unsupported OS. Please install certbot manually."
        exit 1
    fi
fi

# Stop any running containers that might be using port 80
echo "🛑 Stopping containers that might be using port 80..."
docker-compose -f docker-compose.production.yml down web-server || true

# Generate Let's Encrypt certificate
echo "🔐 Generating Let's Encrypt certificate..."
sudo certbot certonly \
    --standalone \
    --non-interactive \
    --agree-tos \
    --email "$EMAIL" \
    --domains "$DOMAIN" \
    --cert-path ./nginx/ssl/cert.pem \
    --key-path ./nginx/ssl/key.pem

# Copy certificates to our SSL directory
echo "📁 Copying certificates..."
sudo cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" ./nginx/ssl/cert.pem
sudo cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" ./nginx/ssl/key.pem

# Set proper permissions
sudo chown $(whoami):$(whoami) ./nginx/ssl/cert.pem ./nginx/ssl/key.pem
chmod 644 ./nginx/ssl/cert.pem
chmod 600 ./nginx/ssl/key.pem

# Create renewal script
echo "📝 Creating certificate renewal script..."
cat > ./scripts/renew-ssl.sh << EOF
#!/bin/bash
# Certificate renewal script
set -e

echo "🔄 Renewing Let's Encrypt certificates..."

# Stop web server
docker-compose -f docker-compose.production.yml stop web-server

# Renew certificates
sudo certbot renew --standalone

# Copy renewed certificates
sudo cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" ./nginx/ssl/cert.pem
sudo cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" ./nginx/ssl/key.pem

# Set permissions
sudo chown \$(whoami):\$(whoami) ./nginx/ssl/cert.pem ./nginx/ssl/key.pem
chmod 644 ./nginx/ssl/cert.pem
chmod 600 ./nginx/ssl/key.pem

# Restart web server
docker-compose -f docker-compose.production.yml up -d web-server

echo "✅ Certificate renewal completed!"
EOF

chmod +x ./scripts/renew-ssl.sh

# Set up automatic renewal
echo "⏰ Setting up automatic renewal..."
(crontab -l 2>/dev/null; echo "0 12 * * * $(pwd)/scripts/renew-ssl.sh >> $(pwd)/logs/ssl-renewal.log 2>&1") | crontab -

echo "✅ Let's Encrypt SSL setup completed!"
echo "📁 Certificate: ./nginx/ssl/cert.pem"
echo "🔑 Private Key: ./nginx/ssl/key.pem"
echo "🔄 Auto-renewal: Set up in crontab"
echo ""
echo "🚀 You can now start the production environment:"
echo "   docker-compose -f docker-compose.production.yml up -d"
