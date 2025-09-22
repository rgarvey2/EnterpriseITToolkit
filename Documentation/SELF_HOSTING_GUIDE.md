# Enterprise IT Toolkit - Self-Hosting Guide

## 🏠 **COMPLETE SELF-HOSTING SETUP**

### **Prerequisites**
- Windows Server 2019/2022 or Windows 10/11
- .NET 8.0 Runtime
- SQL Server (LocalDB, Express, or Full)
- IIS (Optional, for web hosting)
- PowerShell 5.1 or later

---

## 🚀 **METHOD 1: DIRECT EXECUTION (Simplest)**

### **Step 1: Prepare the Application**
```powershell
# Build the application
dotnet publish -c Release -o ./publish --self-contained false

# Or build as self-contained (includes .NET runtime)
dotnet publish -c Release -o ./publish --self-contained true -r win-x64
```

### **Step 2: Configure for Web Access**
```powershell
# Set environment variables
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5000;https://0.0.0.0:5001"

# Run the application
cd ./publish
dotnet EnterpriseITToolkit.dll
```

### **Step 3: Access the Application**
- **Windows Forms App**: Available locally
- **Web Dashboard**: `http://your-server-ip:5000`
- **API Endpoints**: `http://your-server-ip:5000/api/`

---

## 🌐 **METHOD 2: IIS HOSTING (Recommended for Production)**

### **Step 1: Install IIS and ASP.NET Core Hosting Bundle**
```powershell
# Install IIS with required features
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestFiltering, IIS-StaticContent, IIS-DefaultDocument, IIS-DirectoryBrowsing, IIS-ASPNET45

# Download and install ASP.NET Core Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### **Step 2: Create IIS Application**
```powershell
# Create application pool
New-WebAppPool -Name "EnterpriseITToolkit"

# Set application pool settings
Set-ItemProperty -Path "IIS:\AppPools\EnterpriseITToolkit" -Name processModel.identityType -Value ApplicationPoolIdentity
Set-ItemProperty -Path "IIS:\AppPools\EnterpriseITToolkit" -Name managedRuntimeVersion -Value ""

# Create website
New-Website -Name "EnterpriseITToolkit" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\EnterpriseITToolkit" -ApplicationPool "EnterpriseITToolkit"
```

### **Step 3: Deploy Application**
```powershell
# Copy published files to IIS directory
Copy-Item -Path "./publish/*" -Destination "C:\inetpub\wwwroot\EnterpriseITToolkit" -Recurse -Force

# Set permissions
icacls "C:\inetpub\wwwroot\EnterpriseITToolkit" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

### **Step 4: Configure web.config**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\EnterpriseITToolkit.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

---

## 🐳 **METHOD 3: DOCKER CONTAINER**

### **Step 1: Create Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnterpriseITToolkit.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseITToolkit.dll"]
```

### **Step 2: Build and Run Container**
```powershell
# Build Docker image
docker build -t enterprise-it-toolkit .

# Run container
docker run -d -p 80:80 -p 443:443 --name enterprise-it-toolkit -e ASPNETCORE_ENVIRONMENT=Production enterprise-it-toolkit
```

---

## 🔒 **SECURITY CONFIGURATION**

### **1. Windows Firewall Rules**
```powershell
# Allow HTTP traffic
New-NetFirewallRule -DisplayName "Enterprise IT Toolkit HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow

# Allow HTTPS traffic
New-NetFirewallRule -DisplayName "Enterprise IT Toolkit HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Allow custom ports (if using non-standard ports)
New-NetFirewallRule -DisplayName "Enterprise IT Toolkit Custom" -Direction Inbound -Protocol TCP -LocalPort 5000,5001 -Action Allow
```

### **2. IP Restrictions (Application Level)**
```csharp
// In Program.cs or Startup.cs
services.AddCors(options =>
{
    options.AddPolicy("RestrictedAccess", builder =>
    {
        builder.WithOrigins("https://yourdomain.com", "https://admin.yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

### **3. Nginx Reverse Proxy (Optional)**
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl;
    server_name yourdomain.com;
    
    # SSL Configuration
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    
    # IP Restrictions
    allow 192.168.1.0/24;
    allow 10.0.0.0/8;
    deny all;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 🗄️ **DATABASE SETUP**

### **Option 1: SQL Server LocalDB (Simplest)**
```powershell
# LocalDB is included with Visual Studio or SQL Server Express
# Connection string in appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EnterpriseITToolkit;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

### **Option 2: SQL Server Express**
```powershell
# Download and install SQL Server Express
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Create database
sqlcmd -S .\SQLEXPRESS -E -Q "CREATE DATABASE EnterpriseITToolkit"
```

### **Option 3: Full SQL Server**
```powershell
# Install SQL Server with required features
# Create database and configure connection string
```

---

## 📁 **FILE STRUCTURE FOR SELF-HOSTING**

```
C:\EnterpriseITToolkit\
├── appsettings.json          # Configuration
├── appsettings.Production.json
├── EnterpriseITToolkit.dll   # Main application
├── EnterpriseITToolkit.exe   # Windows Forms executable
├── Web\                      # Web interface files
│   └── wwwroot\
│       ├── index.html
│       ├── css\site.css
│       └── js\site.js
├── logs\                     # Log files
└── data\                     # Database files (if using LocalDB)
```

---

## ⚙️ **CONFIGURATION FILES**

### **appsettings.Production.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EnterpriseITToolkit;Trusted_Connection=true;MultipleActiveResultSets=true",
    "Redis": ""
  },
  "JwtSettings": {
    "SecretKey": "your-production-secret-key-here",
    "Issuer": "EnterpriseITToolkit",
    "Audience": "EnterpriseITToolkit",
    "ExpirationMinutes": 60
  },
  "AllowedHosts": "*",
  "CorsOrigins": [
    "https://yourdomain.com",
    "https://admin.yourdomain.com"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 🚀 **DEPLOYMENT SCRIPTS**

### **Complete Self-Hosting Script**
```powershell
# Self-Hosting Deployment Script
param(
    [string]$InstallPath = "C:\EnterpriseITToolkit",
    [string]$Port = "5000",
    [string]$Domain = ""
)

Write-Host "🏠 Enterprise IT Toolkit - Self-Hosting Setup" -ForegroundColor Green

# Create installation directory
New-Item -ItemType Directory -Path $InstallPath -Force

# Build and publish application
Write-Host "📦 Building application..." -ForegroundColor Blue
dotnet publish -c Release -o $InstallPath --self-contained false

# Copy configuration files
Copy-Item "appsettings.json" "$InstallPath\" -Force
Copy-Item "appsettings.Production.json" "$InstallPath\" -Force

# Create startup script
$startupScript = @"
@echo off
cd /d "$InstallPath"
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://0.0.0.0:$Port
dotnet EnterpriseITToolkit.dll
"@

$startupScript | Out-File -FilePath "$InstallPath\start.bat" -Encoding ASCII

# Create Windows Service (Optional)
$serviceScript = @"
sc create "EnterpriseITToolkit" binPath= "$InstallPath\EnterpriseITToolkit.exe" start= auto
sc description "EnterpriseITToolkit" "Enterprise IT Toolkit Management System"
"@

$serviceScript | Out-File -FilePath "$InstallPath\install-service.bat" -Encoding ASCII

# Configure Windows Firewall
Write-Host "🔥 Configuring Windows Firewall..." -ForegroundColor Blue
New-NetFirewallRule -DisplayName "Enterprise IT Toolkit" -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow -ErrorAction SilentlyContinue

Write-Host "✅ Self-hosting setup complete!" -ForegroundColor Green
Write-Host "📁 Installation path: $InstallPath" -ForegroundColor Yellow
Write-Host "🌐 Access URL: http://localhost:$Port" -ForegroundColor Yellow
Write-Host "🚀 To start: Run $InstallPath\start.bat" -ForegroundColor Yellow
```

---

## 🔧 **MAINTENANCE & MONITORING**

### **1. Windows Service Management**
```powershell
# Install as Windows Service
sc create "EnterpriseITToolkit" binPath= "C:\EnterpriseITToolkit\EnterpriseITToolkit.exe" start= auto

# Start/Stop service
sc start "EnterpriseITToolkit"
sc stop "EnterpriseITToolkit"

# Check service status
sc query "EnterpriseITToolkit"
```

### **2. Log Monitoring**
```powershell
# View application logs
Get-Content "C:\EnterpriseITToolkit\logs\*.log" -Tail 50 -Wait

# View Windows Event Logs
Get-EventLog -LogName Application -Source "EnterpriseITToolkit" -Newest 10
```

### **3. Performance Monitoring**
```powershell
# Monitor application performance
Get-Process -Name "EnterpriseITToolkit" | Select-Object CPU, WorkingSet, ProcessName

# Monitor network connections
netstat -an | findstr ":5000"
```

---

## 🌐 **DOMAIN & SSL SETUP**

### **1. Custom Domain Configuration**
```powershell
# Update hosts file for local testing
Add-Content -Path "C:\Windows\System32\drivers\etc\hosts" -Value "127.0.0.1 yourdomain.com"
```

### **2. SSL Certificate (Let's Encrypt)**
```powershell
# Install Certbot for Windows
# https://certbot.eff.org/instructions?ws=other&os=windows

# Generate certificate
certbot certonly --standalone -d yourdomain.com
```

### **3. IIS SSL Configuration**
```powershell
# Bind SSL certificate to IIS site
New-WebBinding -Name "EnterpriseITToolkit" -Protocol https -Port 443 -SslFlags 1
```

---

## 📊 **SELF-HOSTING BENEFITS**

### **✅ Advantages**
- **Complete Control**: Full control over security and configuration
- **Cost Effective**: No monthly hosting fees
- **Network Security**: Complete control over network access
- **Customization**: Any configuration needed
- **Data Privacy**: Data stays on your infrastructure
- **Performance**: Optimized for your hardware

### **⚠️ Considerations**
- **Maintenance**: You're responsible for updates and maintenance
- **Backup**: Need to implement backup strategies
- **Monitoring**: Need to set up monitoring and alerting
- **Security**: Responsible for security updates and patches
- **Availability**: Need to ensure high availability

---

## 🎯 **RECOMMENDED SELF-HOSTING SETUP**

### **For Small Office (1-10 users)**
- **Method**: Direct execution with Windows Service
- **Database**: SQL Server LocalDB
- **Access**: Local network only
- **SSL**: Self-signed certificate

### **For Medium Office (10-50 users)**
- **Method**: IIS hosting
- **Database**: SQL Server Express
- **Access**: VPN or IP restrictions
- **SSL**: Let's Encrypt certificate

### **For Enterprise (50+ users)**
- **Method**: IIS with load balancing
- **Database**: SQL Server Standard/Enterprise
- **Access**: Custom domain with enterprise SSL
- **Monitoring**: Application Insights or custom monitoring

---

## 🚀 **QUICK START COMMANDS**

```powershell
# 1. Build and deploy
dotnet publish -c Release -o C:\EnterpriseITToolkit --self-contained false

# 2. Configure and start
cd C:\EnterpriseITToolkit
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5000"
dotnet EnterpriseITToolkit.dll

# 3. Access the application
# Windows Forms: Available locally
# Web Dashboard: http://your-server-ip:5000
# API: http://your-server-ip:5000/api/
```

**Your Enterprise IT Toolkit is now self-hosted and ready for use!** 🎉
