#!/bin/bash

# SSL Certificate Generation Script for Production
# This script generates self-signed certificates for development/testing
# For production, use Let's Encrypt or a proper CA

set -e

SSL_DIR="./nginx/ssl"
DOMAIN=${1:-localhost}

echo "🔐 Generating SSL certificates for domain: $DOMAIN"

# Create SSL directory if it doesn't exist
mkdir -p "$SSL_DIR"

# Generate private key
echo "📝 Generating private key..."
openssl genrsa -out "$SSL_DIR/key.pem" 2048

# Generate certificate signing request
echo "📝 Generating certificate signing request..."
openssl req -new -key "$SSL_DIR/key.pem" -out "$SSL_DIR/cert.csr" -subj "/C=US/ST=State/L=City/O=Organization/CN=$DOMAIN"

# Generate self-signed certificate
echo "📝 Generating self-signed certificate..."
openssl x509 -req -days 365 -in "$SSL_DIR/cert.csr" -signkey "$SSL_DIR/key.pem" -out "$SSL_DIR/cert.pem"

# Clean up CSR file
rm "$SSL_DIR/cert.csr"

# Set proper permissions
chmod 600 "$SSL_DIR/key.pem"
chmod 644 "$SSL_DIR/cert.pem"

echo "✅ SSL certificates generated successfully!"
echo "📁 Certificate: $SSL_DIR/cert.pem"
echo "🔑 Private Key: $SSL_DIR/key.pem"
echo ""
echo "⚠️  Note: These are self-signed certificates for development/testing."
echo "   For production, use Let's Encrypt or a proper Certificate Authority."
echo ""
echo "🚀 To use Let's Encrypt in production, run:"
echo "   ./scripts/setup-letsencrypt.sh yourdomain.com"
