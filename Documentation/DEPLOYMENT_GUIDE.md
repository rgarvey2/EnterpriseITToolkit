# Enterprise IT Toolkit v4.0 - Deployment Guide

## 🚀 **Deployment Overview**

This guide provides comprehensive instructions for deploying the Enterprise IT Toolkit v4.0 in various environments, from development to production.

## 📋 **Prerequisites**

### **System Requirements**
- **Operating System**: Windows 10/11 or Windows Server 2019/2022
- **.NET Runtime**: .NET 8.0 Runtime (included in self-contained deployment)
- **Memory**: Minimum 4GB RAM, Recommended 8GB+
- **Disk Space**: 500MB for application, 2GB+ for logs and data
- **Permissions**: Administrator privileges for full functionality

### **Network Requirements**
- **Active Directory**: Optional, for domain authentication
- **Internet Access**: For Windows Updates and cloud features
- **Firewall**: Allow outbound HTTPS (443) and HTTP (80) traffic

## 🔧 **Installation Methods**

### **Method 1: Self-Contained Deployment (Recommended)**

1. **Download the Application**
   ```bash
   # Extract the self-contained package
   tar -xzf EnterpriseITToolkit-v4.0-win-x64.tar.gz
   cd EnterpriseITToolkit-v4.0
   ```

2. **Run the Application**
   ```bash
   # Direct execution
   ./EnterpriseITToolkit.exe
   
   # Or with specific configuration
   ./EnterpriseITToolkit.exe --config=production.json
   ```

### **Method 2: Framework-Dependent Deployment**

1. **Install .NET 8.0 Runtime**
   ```powershell
   # Download and install from Microsoft
   # https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. **Deploy Application Files**
   ```bash
   # Copy application files to target directory
   cp -r EnterpriseITToolkit/* /opt/EnterpriseITToolkit/
   ```

3. **Run the Application**
   ```bash
   dotnet /opt/EnterpriseITToolkit/EnterpriseITToolkit.dll
   ```

## ⚙️ **Configuration**

### **Environment-Specific Configuration**

#### **Development Environment**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning"
    }
  },
  "SecuritySettings": {
    "RequireAuthentication": false,
    "AuditLogPath": "C:\\Logs\\Audit\\Dev",
    "MaxLoginAttempts": 5,
    "SessionTimeoutMinutes": 60
  }
}
```

#### **Production Environment**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "SecuritySettings": {
    "RequireAuthentication": true,
    "AuditLogPath": "C:\\Logs\\Audit\\Prod",
    "MaxLoginAttempts": 3,
    "SessionTimeoutMinutes": 30
  }
}
```

### **Security Configuration**

#### **Active Directory Integration**
```json
{
  "SecuritySettings": {
    "ActiveDirectory": {
      "Domain": "yourdomain.com",
      "LdapPath": "LDAP://yourdomain.com/DC=yourdomain,DC=com",
      "GroupFilter": "(&(objectClass=group)(cn=ITTechnicians))"
    }
  }
}
```

#### **Network Security**
```json
{
  "NetworkSettings": {
    "AllowedHosts": [
      "8.8.8.8",
      "1.1.1.1",
      "your-internal-server.com"
    ],
    "BlockedPorts": [22, 23, 135, 139, 445, 3389],
    "MaxConcurrentPings": 10,
    "DefaultTimeout": 5000
  }
}
```

## 🔐 **Security Setup**

### **1. Authentication Configuration**

#### **Remove Default Test Credentials (Production)**
```csharp
// In AuthenticationService.cs, comment out or remove:
private bool IsDefaultTestCredentials(string username, string password)
{
    // DISABLE FOR PRODUCTION
    return false;
}
```

#### **Enable Strong Authentication**
```json
{
  "SecuritySettings": {
    "PasswordPolicy": {
      "MinLength": 12,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireNumbers": true,
      "RequireSpecialChars": true
    },
    "SessionSecurity": {
      "EncryptTokens": true,
      "TokenExpirationMinutes": 30,
      "RefreshTokenEnabled": true
    }
  }
}
```

### **2. Audit Logging Setup**

#### **Create Audit Log Directory**
```powershell
# Create secure audit log directory
New-Item -ItemType Directory -Path "C:\Logs\Audit" -Force
icacls "C:\Logs\Audit" /grant "Administrators:(OI)(CI)F" /T
icacls "C:\Logs\Audit" /grant "SYSTEM:(OI)(CI)F" /T
icacls "C:\Logs\Audit" /remove "Users" /T
```

#### **Configure Log Rotation**
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:\\Logs\\Audit\\audit-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true
        }
      }
    ]
  }
}
```

## 🚀 **Service Installation (Windows)**

### **Install as Windows Service**

1. **Create Service Wrapper**
   ```csharp
   // Create ServiceWrapper.cs
   public class ServiceWrapper : ServiceBase
   {
       private IHost _host;
       
       protected override void OnStart(string[] args)
       {
           _host = CreateHostBuilder(args).Build();
           _host.Start();
       }
       
       protected override void OnStop()
       {
           _host?.StopAsync().GetAwaiter().GetResult();
           _host?.Dispose();
       }
   }
   ```

2. **Install Service**
   ```powershell
   # Install as Windows Service
   sc create "EnterpriseITToolkit" binPath="C:\Program Files\EnterpriseITToolkit\EnterpriseITToolkit.exe" start=auto
   sc description "EnterpriseITToolkit" "Enterprise IT Management Toolkit"
   sc start "EnterpriseITToolkit"
   ```

### **Configure Service Recovery**
```powershell
# Configure service recovery options
sc failure "EnterpriseITToolkit" reset=86400 actions=restart/5000/restart/10000/restart/20000
```

## 📊 **Monitoring and Maintenance**

### **Health Checks**

#### **Application Health Check**
```csharp
// Add to Program.cs
services.AddHealthChecks()
    .AddCheck<ConfigurationHealthCheck>("configuration")
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<NetworkHealthCheck>("network");
```

#### **Performance Monitoring**
```json
{
  "SystemSettings": {
    "HealthCheckInterval": 300,
    "PerformanceMonitoring": {
      "Enabled": true,
      "CpuThreshold": 80,
      "MemoryThreshold": 85,
      "DiskThreshold": 90
    }
  }
}
```

### **Log Management**

#### **Log Rotation Script**
```powershell
# Create log rotation script
$LogPath = "C:\Logs\EnterpriseITToolkit"
$MaxAge = 30

Get-ChildItem -Path $LogPath -File | Where-Object {
    $_.LastWriteTime -lt (Get-Date).AddDays(-$MaxAge)
} | Remove-Item -Force

Write-Host "Log cleanup completed"
```

#### **Automated Backup**
```powershell
# Backup configuration and logs
$BackupPath = "C:\Backups\EnterpriseITToolkit"
$Date = Get-Date -Format "yyyyMMdd"

New-Item -ItemType Directory -Path "$BackupPath\$Date" -Force
Copy-Item "C:\Logs\EnterpriseITToolkit\*" "$BackupPath\$Date\Logs\" -Recurse
Copy-Item "appsettings.json" "$BackupPath\$Date\"
```

## 🔄 **Updates and Upgrades**

### **In-Place Upgrade Process**

1. **Backup Current Installation**
   ```powershell
   # Stop service
   sc stop "EnterpriseITToolkit"
   
   # Backup current version
   Copy-Item "C:\Program Files\EnterpriseITToolkit" "C:\Backups\EnterpriseITToolkit-v3.0" -Recurse
   ```

2. **Deploy New Version**
   ```powershell
   # Extract new version
   Expand-Archive "EnterpriseITToolkit-v4.0.zip" -DestinationPath "C:\Program Files\EnterpriseITToolkit"
   
   # Update configuration if needed
   # Compare and merge appsettings.json changes
   ```

3. **Start Updated Service**
   ```powershell
   # Start service
   sc start "EnterpriseITToolkit"
   
   # Verify functionality
   # Check logs and test key features
   ```

### **Rollback Procedure**
```powershell
# If upgrade fails, rollback
sc stop "EnterpriseITToolkit"
Remove-Item "C:\Program Files\EnterpriseITToolkit" -Recurse -Force
Copy-Item "C:\Backups\EnterpriseITToolkit-v3.0" "C:\Program Files\EnterpriseITToolkit" -Recurse
sc start "EnterpriseITToolkit"
```

## 🛡️ **Security Hardening**

### **File System Permissions**
```powershell
# Secure application directory
icacls "C:\Program Files\EnterpriseITToolkit" /grant "Administrators:(OI)(CI)F" /T
icacls "C:\Program Files\EnterpriseITToolkit" /grant "SYSTEM:(OI)(CI)F" /T
icacls "C:\Program Files\EnterpriseITToolkit" /remove "Users" /T
icacls "C:\Program Files\EnterpriseITToolkit" /remove "Authenticated Users" /T
```

### **Network Security**
```powershell
# Configure Windows Firewall
New-NetFirewallRule -DisplayName "EnterpriseITToolkit" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow
New-NetFirewallRule -DisplayName "EnterpriseITToolkit-Outbound" -Direction Outbound -Protocol TCP -Action Allow
```

### **Registry Security**
```powershell
# Secure registry keys
reg add "HKLM\SOFTWARE\EnterpriseITToolkit" /v "SecureMode" /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\EnterpriseITToolkit" /v "AuditLevel" /t REG_DWORD /d 3 /f
```

## 📈 **Performance Optimization**

### **Memory Optimization**
```json
{
  "SystemSettings": {
    "MemoryManagement": {
      "GarbageCollectionMode": "Server",
      "HeapSizeLimit": "2GB",
      "CacheSizeLimit": "512MB"
    }
  }
}
```

### **Network Optimization**
```json
{
  "NetworkSettings": {
    "ConnectionPooling": {
      "MaxConnections": 100,
      "ConnectionTimeout": 30000,
      "KeepAlive": true
    }
  }
}
```

## 🔍 **Troubleshooting**

### **Common Issues**

#### **Service Won't Start**
```powershell
# Check service status
sc query "EnterpriseITToolkit"

# Check event logs
Get-EventLog -LogName Application -Source "EnterpriseITToolkit" -Newest 10

# Check configuration
Test-Path "C:\Program Files\EnterpriseITToolkit\appsettings.json"
```

#### **Authentication Issues**
```powershell
# Check Active Directory connectivity
Test-NetConnection -ComputerName "your-domain-controller" -Port 389

# Verify user permissions
whoami /groups
```

#### **Performance Issues**
```powershell
# Check system resources
Get-Process "EnterpriseITToolkit" | Select-Object CPU, WorkingSet, Handles

# Check disk space
Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, Size, FreeSpace
```

### **Log Analysis**
```powershell
# Search for errors
Select-String -Path "C:\Logs\EnterpriseITToolkit\*.log" -Pattern "ERROR" | Select-Object -First 20

# Monitor real-time logs
Get-Content "C:\Logs\EnterpriseITToolkit\log-$(Get-Date -Format 'yyyyMMdd').txt" -Wait -Tail 10
```

## 📞 **Support and Maintenance**

### **Support Contacts**
- **Technical Support**: support@enterpriseittoolkit.com
- **Documentation**: https://docs.enterpriseittoolkit.com
- **Community Forum**: https://community.enterpriseittoolkit.com

### **Maintenance Schedule**
- **Daily**: Log rotation and cleanup
- **Weekly**: Performance monitoring review
- **Monthly**: Security audit and updates
- **Quarterly**: Full system backup and testing

### **Version Information**
- **Current Version**: 4.0.0
- **Release Date**: 2024-01-15
- **Next Update**: 2024-04-15
- **End of Support**: 2026-01-15

---

## ✅ **Deployment Checklist**

- [ ] System requirements verified
- [ ] Application files deployed
- [ ] Configuration files updated
- [ ] Security settings configured
- [ ] Service installed and started
- [ ] Health checks passing
- [ ] Monitoring configured
- [ ] Backup procedures tested
- [ ] Documentation updated
- [ ] Team trained on new features

**Deployment completed successfully!** 🎉
