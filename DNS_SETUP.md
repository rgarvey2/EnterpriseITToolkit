# DNS Configuration for app.techenttools.com

This guide will help you configure DNS records for your Enterprise IT Toolkit deployment.

## 🌐 Required DNS Records

### A Record (Primary)
```
Type: A
Name: app
Domain: techenttools.com
Value: [YOUR_SERVER_IP]
TTL: 300 (5 minutes)
```

### CNAME Record (Optional - for www subdomain)
```
Type: CNAME
Name: www.app
Domain: techenttools.com
Value: app.techenttools.com
TTL: 300 (5 minutes)
```

## 🔍 Finding Your Server IP

### Method 1: From the server
```bash
curl -s ifconfig.me
```

### Method 2: From your hosting provider
Check your server's public IP address in your hosting control panel.

### Method 3: From command line
```bash
# Linux/Mac
dig +short myip.opendns.com @resolver1.opendns.com

# Windows
nslookup myip.opendns.com resolver1.opendns.com
```

## 📋 DNS Provider Instructions

### Cloudflare
1. Log in to Cloudflare dashboard
2. Select `techenttools.com` domain
3. Go to **DNS** → **Records**
4. Click **Add record**
5. Set Type: `A`, Name: `app`, IPv4 address: `[YOUR_SERVER_IP]`
6. Click **Save**

### GoDaddy
1. Log in to GoDaddy account
2. Go to **My Products** → **DNS**
3. Find `techenttools.com` and click **Manage**
4. Click **Add** in the Records section
5. Set Type: `A`, Host: `app`, Points to: `[YOUR_SERVER_IP]`
6. Click **Save**

### Namecheap
1. Log in to Namecheap account
2. Go to **Domain List** → **Manage** for `techenttools.com`
3. Go to **Advanced DNS** tab
4. Click **Add New Record**
5. Set Type: `A Record`, Host: `app`, Value: `[YOUR_SERVER_IP]`
6. Click **Save All Changes**

### Route 53 (AWS)
1. Log in to AWS Console
2. Go to **Route 53** → **Hosted zones**
3. Select `techenttools.com`
4. Click **Create record**
5. Set Record name: `app`, Record type: `A`, Value: `[YOUR_SERVER_IP]`
6. Click **Create records**

## ⏱️ DNS Propagation

DNS changes can take time to propagate globally:

- **TTL 300 (5 minutes)**: Changes visible within 5-10 minutes
- **TTL 3600 (1 hour)**: Changes visible within 1-2 hours
- **TTL 86400 (24 hours)**: Changes visible within 24-48 hours

### Check DNS Propagation

```bash
# Check if DNS is resolving
nslookup app.techenttools.com

# Check from different locations
dig @8.8.8.8 app.techenttools.com
dig @1.1.1.1 app.techenttools.com
```

### Online DNS Checkers
- [whatsmydns.net](https://www.whatsmydns.net/)
- [dnschecker.org](https://dnschecker.org/)
- [dnsmap.io](https://dnsmap.io/)

## 🔧 SSL Certificate Setup

Once DNS is configured, set up SSL certificates:

### Let's Encrypt (Recommended)
```bash
# Stop web server temporarily
docker-compose -f docker-compose.production.yml stop web-server

# Generate Let's Encrypt certificate
sudo certbot certonly --standalone -d app.techenttools.com --email admin@techenttools.com --agree-tos

# Copy certificates
sudo cp /etc/letsencrypt/live/app.techenttools.com/fullchain.pem ./nginx/ssl/cert.pem
sudo cp /etc/letsencrypt/live/app.techenttools.com/privkey.pem ./nginx/ssl/key.pem
sudo chown $(whoami):$(whoami) ./nginx/ssl/cert.pem ./nginx/ssl/key.pem

# Restart web server
docker-compose -f docker-compose.production.yml up -d web-server
```

### Automated SSL Setup
```bash
chmod +x scripts/setup-letsencrypt.sh
./scripts/setup-letsencrypt.sh app.techenttools.com admin@techenttools.com
```

## 🧪 Testing DNS Configuration

### 1. Check DNS Resolution
```bash
nslookup app.techenttools.com
```

### 2. Test HTTP Connection
```bash
curl -I http://app.techenttools.com
```

### 3. Test HTTPS Connection
```bash
curl -I https://app.techenttools.com
```

### 4. Test API Endpoint
```bash
curl https://app.techenttools.com/api/health
```

## 🚨 Troubleshooting

### DNS Not Resolving
1. Check if DNS record was created correctly
2. Wait for DNS propagation (up to 48 hours)
3. Clear DNS cache: `sudo systemctl flush-dns` (Linux) or `ipconfig /flushdns` (Windows)

### SSL Certificate Issues
1. Ensure DNS is resolving correctly
2. Check if port 80 is accessible for Let's Encrypt validation
3. Verify domain ownership

### Connection Refused
1. Check if server is running: `docker-compose -f docker-compose.production.yml ps`
2. Check firewall settings
3. Verify server IP address

## 📞 Support

If you encounter issues:
1. Check DNS propagation status
2. Verify server IP address
3. Test connectivity from different locations
4. Check server logs: `docker-compose -f docker-compose.production.yml logs -f`

---

**Note**: DNS changes can take up to 48 hours to propagate globally, but typically resolve within 5-30 minutes with modern DNS providers.
