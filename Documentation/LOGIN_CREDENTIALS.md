# Enterprise IT Toolkit - Login Credentials

## Authentication System

The Enterprise IT Toolkit uses a multi-layered authentication system:

1. **Active Directory Authentication** (Primary)
2. **Local Machine Authentication** (Secondary)
3. **Default Test Credentials** (Development/Testing)

## Default Test Credentials

For development and testing purposes, the following default credentials are available:

| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `admin` | `admin123` | Administrator | Full system access |
| `administrator` | `admin123` | Administrator | Full system access |
| `tech` | `tech123` | Technician | Standard technician access |
| `technician` | `tech123` | Technician | Standard technician access |
| `demo` | `demo123` | Technician | Demo account |
| `test` | `test123` | Technician | Test account |

## Role-Based Access Control

### Administrator Role
- Full system access
- All permissions including:
  - AD_USER_MANAGEMENT
  - SYSTEM_CONFIGURATION
  - NETWORK_TOOLS
  - SECURITY_TOOLS
  - All other permissions

### Technician Role
- Standard technician access
- Permissions excluding:
  - AD_USER_MANAGEMENT
  - SYSTEM_CONFIGURATION
- Includes all other permissions

### Read-Only Role
- Limited to view/read operations
- Permissions containing "VIEW" or "READ"

## Production Deployment

**⚠️ IMPORTANT SECURITY NOTICE:**

For production deployment, you should:

1. **Disable default test credentials** by modifying the `IsDefaultTestCredentials` method in `AuthenticationService.cs`
2. **Configure proper Active Directory integration**
3. **Set up proper user accounts** in your domain
4. **Enable audit logging** for all authentication events
5. **Use strong password policies**

## Authentication Flow

1. User enters credentials in login form
2. System attempts Active Directory authentication
3. If AD fails, attempts local machine authentication
4. If both fail, checks default test credentials (development only)
5. On success, creates session token and assigns roles
6. Session expires after configured timeout (default: 30 minutes)

## Security Features

- **Session Management**: Secure session tokens with expiration
- **Audit Logging**: All authentication events are logged
- **Password Hashing**: Uses PBKDF2 with salt for stored passwords
- **Role-Based Access**: Granular permission system
- **Input Validation**: All inputs are validated and sanitized

## Troubleshooting

### Common Issues

1. **"Invalid username or password"**
   - Check if using correct domain format (DOMAIN\username)
   - Verify Active Directory connectivity
   - Try default test credentials for testing

2. **"Authentication service error"**
   - Check application logs
   - Verify Active Directory service is running
   - Check network connectivity to domain controller

3. **"Session expired"**
   - Re-login with credentials
   - Check session timeout configuration

### Log Locations

- Application logs: `C:\Logs\EnterpriseITToolkit\`
- Audit logs: `C:\Logs\Audit\`
- Security events: Windows Event Log

## Configuration

Authentication settings can be configured in `appsettings.json`:

```json
{
  "SecuritySettings": {
    "RequireAuthentication": true,
    "MaxLoginAttempts": 3,
    "SessionTimeoutMinutes": 30,
    "AuditLogPath": "C:\\Logs\\Audit"
  }
}
```
