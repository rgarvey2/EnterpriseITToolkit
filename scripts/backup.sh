#!/bin/bash

# Database Backup Script for Production
# This script creates automated backups of the PostgreSQL database

set -e

# Configuration
BACKUP_DIR="/backups"
DB_HOST="postgres"
DB_NAME="enterprise_toolkit"
DB_USER="enterprise_user"
RETENTION_DAYS=${BACKUP_RETENTION_DAYS:-30}

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Generate backup filename with timestamp
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/enterprise_toolkit_backup_$TIMESTAMP.sql"

echo "🗄️  Starting database backup..."
echo "📁 Backup file: $BACKUP_FILE"

# Create database backup
pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" \
    --verbose \
    --clean \
    --if-exists \
    --create \
    --format=plain \
    --file="$BACKUP_FILE"

# Compress the backup
echo "📦 Compressing backup..."
gzip "$BACKUP_FILE"
BACKUP_FILE="$BACKUP_FILE.gz"

echo "✅ Backup completed: $BACKUP_FILE"

# Clean up old backups
echo "🧹 Cleaning up old backups (older than $RETENTION_DAYS days)..."
find "$BACKUP_DIR" -name "enterprise_toolkit_backup_*.sql.gz" -type f -mtime +$RETENTION_DAYS -delete

echo "✅ Backup process completed successfully!"

# Log backup info
echo "$(date): Backup completed - $BACKUP_FILE" >> "$BACKUP_DIR/backup.log"
