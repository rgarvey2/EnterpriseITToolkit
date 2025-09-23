# Enterprise IT Toolkit - Production Deployment Guide

This guide will help you deploy the Enterprise IT Toolkit to production using Docker containers for self-hosting.

## 🚀 Quick Start

### Prerequisites

- Docker and Docker Compose installed
- Domain name (for SSL certificates)
- Basic knowledge of Docker and Linux administration

### 1. Clone and Setup

```bash
git clone https://github.com/yourusername/EnterpriseITToolkit.git
cd EnterpriseITToolkit
```

### 2. Configure Environment

```bash
# Copy the production environment template
cp production.env.example production.env

# Edit the configuration
nano production.env
```

**Important:** Change all default passwords and secrets!

### 3. Generate SSL Certificates

#### Option A: Self-Signed (Development/Testing)
```bash
chmod +x scripts/generate-ssl.sh
./scripts/generate-ssl.sh localhost
```

#### Option B: Let's Encrypt (Production)
```bash
chmod +x scripts/setup-letsencrypt.sh
./scripts/setup-letsencrypt.sh yourdomain.com admin@yourdomain.com
```

### 4. Deploy to Production

```bash
chmod +x scripts/deploy-production.sh
./scripts/deploy-production.sh
```

## 📋 Production Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Nginx (443)   │────│  API Server     │────│   PostgreSQL    │
│   Web Server    │    │  (Node.js)      │    │   Database      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Grafana       │    │    Redis        │    │   Prometheus    │
│   Monitoring    │    │    Cache        │    │   Metrics       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🔧 Services Overview

| Service | Port | Description | Access |
|---------|------|-------------|---------|
| Nginx | 80, 443 | Web server with SSL termination | https://yourdomain.com |
| API Server | 5004 | REST API backend | http://yourdomain.com:5004 |
| PostgreSQL | 5432 | Primary database | Internal only |
| Redis | 6379 | Caching and sessions | Internal only |
| Grafana | 3000 | Monitoring dashboard | https://yourdomain.com/grafana |
| Prometheus | 9090 | Metrics collection | https://yourdomain.com/prometheus |
| PHP Admin | 8080 | Database administration | https://yourdomain.com/admin |

## 🔐 Security Configuration

### SSL/TLS Setup

1. **Self-Signed Certificates** (Development):
   ```bash
   ./scripts/generate-ssl.sh localhost
   ```

2. **Let's Encrypt** (Production):
   ```bash
   ./scripts/setup-letsencrypt.sh yourdomain.com admin@yourdomain.com
   ```

### Firewall Configuration

```bash
# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow SSH (if needed)
sudo ufw allow 22/tcp

# Enable firewall
sudo ufw enable
```

### Environment Security

Edit `production.env` and change:
- `DB_PASSWORD` - Database password
- `REDIS_PASSWORD` - Redis password  
- `JWT_SECRET` - JWT signing secret
- `GRAFANA_PASSWORD` - Grafana admin password

## 📊 Monitoring and Logging

### View Logs

```bash
# All services
docker-compose -f docker-compose.production.yml logs -f

# Specific service
docker-compose -f docker-compose.production.yml logs -f api-server
```

### Access Monitoring

- **Grafana**: https://yourdomain.com/grafana
  - Username: `admin`
  - Password: `$GRAFANA_PASSWORD`

- **Prometheus**: https://yourdomain.com/prometheus

### Health Checks

```bash
# Check all services
docker-compose -f docker-compose.production.yml ps

# Test API health
curl https://yourdomain.com/api/health

# Test web server
curl https://yourdomain.com/health
```

## 🔄 Backup and Recovery

### Automated Backups

Backups run automatically daily at 2 AM. Manual backup:

```bash
docker-compose -f docker-compose.production.yml exec backup /backup.sh
```

### Restore from Backup

```bash
# Stop services
docker-compose -f docker-compose.production.yml down

# Restore database
docker-compose -f docker-compose.production.yml up -d postgres
sleep 10
docker-compose -f docker-compose.production.yml exec postgres psql -U enterprise_user -d enterprise_toolkit < /backups/backup_file.sql

# Start all services
docker-compose -f docker-compose.production.yml up -d
```

## 🛠️ Maintenance

### Update Application

```bash
# Pull latest changes
git pull origin main

# Rebuild and restart
docker-compose -f docker-compose.production.yml down
docker-compose -f docker-compose.production.yml up -d --build
```

### SSL Certificate Renewal

Let's Encrypt certificates auto-renew via cron job. Manual renewal:

```bash
./scripts/renew-ssl.sh
```

### Database Maintenance

```bash
# Connect to database
docker-compose -f docker-compose.production.yml exec postgres psql -U enterprise_user -d enterprise_toolkit

# Run maintenance commands
VACUUM ANALYZE;
```

## 🚨 Troubleshooting

### Common Issues

1. **SSL Certificate Errors**:
   ```bash
   # Regenerate certificates
   ./scripts/generate-ssl.sh yourdomain.com
   ```

2. **Database Connection Issues**:
   ```bash
   # Check database status
   docker-compose -f docker-compose.production.yml logs postgres
   ```

3. **API Server Not Responding**:
   ```bash
   # Check API logs
   docker-compose -f docker-compose.production.yml logs api-server
   ```

4. **Port Conflicts**:
   ```bash
   # Check what's using ports
   sudo netstat -tulpn | grep :80
   sudo netstat -tulpn | grep :443
   ```

### Performance Optimization

1. **Increase Worker Processes**:
   Edit `nginx/nginx.production.conf`:
   ```nginx
   worker_processes auto;
   worker_connections 2048;
   ```

2. **Enable Gzip Compression**:
   Already enabled in production config.

3. **Database Optimization**:
   ```sql
   -- Increase shared_buffers
   ALTER SYSTEM SET shared_buffers = '256MB';
   ALTER SYSTEM SET effective_cache_size = '1GB';
   ```

## 📞 Support

For issues and support:
- Check logs: `docker-compose -f docker-compose.production.yml logs -f`
- Review this documentation
- Check GitHub issues
- Contact system administrator

## 🔄 Production Checklist

- [ ] Environment variables configured
- [ ] SSL certificates installed
- [ ] Firewall configured
- [ ] Monitoring set up
- [ ] Backups configured
- [ ] Health checks passing
- [ ] Performance optimized
- [ ] Security hardened
- [ ] Documentation updated
- [ ] Team trained on maintenance

---

**Note**: This is a production-ready setup. Ensure you have proper backups and monitoring in place before deploying to production.
