# Enterprise IT Toolkit - Docker Web Setup

This folder contains the Docker web-only setup for the Enterprise IT Toolkit, providing easy cross-platform deployment.

## 🚀 Quick Start

### Option 1: Web-Only (Recommended)
```powershell
.\setup-docker-web.ps1 -WebOnly
```

### Option 2: Full Stack (Web + Database + Redis)
```powershell
.\setup-docker-web.ps1 -FullStack
```

### Option 3: Manual Docker Compose
```powershell
# Web only
docker-compose up -d

# With database and Redis
docker-compose --profile database --profile cache up -d
```

## 🌐 Access URLs

- **Web Dashboard**: http://localhost:5000
- **Health Check**: http://localhost:5000/health
- **Database** (if enabled): localhost:1433
- **Redis** (if enabled): localhost:6379

## 📋 Management Commands

```powershell
# View logs
docker logs enterprise-it-toolkit-web

# Follow logs in real-time
docker logs -f enterprise-it-toolkit-web

# Stop container
docker stop enterprise-it-toolkit-web

# Remove container
docker rm enterprise-it-toolkit-web

# Stop all services
docker-compose down
```

## 🏗️ Architecture

### Web-Only Mode
- **Container**: Single container with web server
- **Size**: ~200MB
- **Features**: Web dashboard, static file serving
- **Use Case**: Quick deployment, testing, simple hosting

### Full Stack Mode
- **Containers**: Web server + SQL Server + Redis
- **Size**: ~2GB
- **Features**: Full database support, caching, enterprise features
- **Use Case**: Production deployment, full functionality

## 🔧 Configuration

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80;https://+:443
```

### Volume Mounts
```bash
./data:/app/data          # Application data
./logs:/app/logs          # Log files
```

## 📁 File Structure

```
Docker-Web/
├── Dockerfile                 # Docker image definition
├── docker-compose.yml         # Docker Compose configuration
├── setup-docker-web.ps1      # Setup script
├── WebServer/                # Web server project
├── web-files/                # Static web files
├── data/                     # Application data (created)
└── logs/                     # Log files (created)
```

## 🎯 Benefits

✅ **Cross-Platform**: Runs on Windows, Linux, macOS
✅ **Small Size**: Minimal container footprint
✅ **Fast Startup**: Quick deployment and scaling
✅ **Easy Management**: Simple Docker commands
✅ **Cloud Ready**: Deploy to any cloud platform
✅ **Web Access**: Access from anywhere via browser

## 🔄 Updates

To update the web files:
1. Copy new files to `web-files/` directory
2. Restart the container: `docker restart enterprise-it-toolkit-web`

## 🆚 vs Desktop App

| Feature | Docker Web | Desktop App |
|---------|------------|-------------|
| **Platform** | Cross-platform | Windows only |
| **UI** | Web browser | Windows Forms |
| **Deployment** | Easy (Docker) | Manual install |
| **Remote Access** | ✅ Yes | ❌ Local only |
| **Full Features** | ⚠️ Limited | ✅ Complete |
| **System Access** | ⚠️ Limited | ✅ Full |

## 🎉 Conclusion

This Docker web setup provides an easy way to deploy the Enterprise IT Toolkit web dashboard across any platform, making it accessible from anywhere while maintaining the full desktop application for local administration.
