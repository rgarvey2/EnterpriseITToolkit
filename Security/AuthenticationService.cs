using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace EnterpriseITToolkit.Security
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);
        Task<bool> ValidateSessionAsync(string sessionToken);
        Task LogoutAsync(string sessionToken);
        Task<List<TechnicianRole>> GetUserRolesAsync(string username);
        Task<bool> HasPermissionAsync(string username, string permission);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly Dictionary<string, UserSession> _activeSessions;
        private readonly List<TechnicianRole> _roles;
        private readonly List<Permission> _permissions;

        public AuthenticationService(ILogger<AuthenticationService> logger)
        {
            _logger = logger;
            _activeSessions = new Dictionary<string, UserSession>();
            _permissions = InitializePermissions();
            _roles = InitializeRoles();
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("Authentication attempt for user: {Username}", username);

                // Validate input
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    AuditLogger.LogAuthenticationEvent(_logger, "LOGIN_ATTEMPT", username, false, "Empty credentials");
                    return new AuthenticationResult { Success = false, Error = "Invalid credentials" };
                }

                // Check if user is in Active Directory or local system
                bool isValidUser = await ValidateUserCredentialsAsync(username, password);
                
                if (!isValidUser)
                {
                    AuditLogger.LogAuthenticationEvent(_logger, "LOGIN_ATTEMPT", username, false, "Invalid credentials");
                    return new AuthenticationResult { Success = false, Error = "Invalid username or password" };
                }

                // Check if user has technician role
                var userRoles = await GetUserRolesAsync(username);
                if (!userRoles.Any(r => r.Name == "Technician" || r.Name == "Administrator"))
                {
                    AuditLogger.LogAuthenticationEvent(_logger, "LOGIN_ATTEMPT", username, false, "Insufficient privileges");
                    return new AuthenticationResult { Success = false, Error = "Access denied. Technician privileges required." };
                }

                // Create session
                var sessionToken = GenerateSessionToken();
                var session = new UserSession
                {
                    Username = username,
                    SessionToken = sessionToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(8), // 8-hour session
                    Roles = userRoles,
                    LastActivity = DateTime.UtcNow
                };

                _activeSessions[sessionToken] = session;

                AuditLogger.LogAuthenticationEvent(_logger, "LOGIN_SUCCESS", username, true);
                _logger.LogInformation("User {Username} authenticated successfully with session {SessionToken}", username, sessionToken);

                return new AuthenticationResult
                {
                    Success = true,
                    SessionToken = sessionToken,
                    Username = username,
                    Roles = userRoles.Select(r => r.Name).ToList(),
                    ExpiresAt = session.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {Username}", username);
                AuditLogger.LogAuthenticationEvent(_logger, "LOGIN_ERROR", username, false, ex.Message);
                return new AuthenticationResult { Success = false, Error = "Authentication service error" };
            }
        }

        public Task<bool> ValidateSessionAsync(string sessionToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionToken) || !_activeSessions.ContainsKey(sessionToken))
                {
                    return Task.FromResult(false);
                }

                var session = _activeSessions[sessionToken];
                
                // Check if session is expired
                if (session.ExpiresAt < DateTime.UtcNow)
                {
                    _activeSessions.Remove(sessionToken);
                    AuditLogger.LogAuthenticationEvent(_logger, "SESSION_EXPIRED", session.Username, false);
                    return Task.FromResult(false);
                }

                // Update last activity
                session.LastActivity = DateTime.UtcNow;
                
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session: {SessionToken}", sessionToken);
                return Task.FromResult(false);
            }
        }

        public Task LogoutAsync(string sessionToken)
        {
            try
            {
                if (_activeSessions.ContainsKey(sessionToken))
                {
                    var session = _activeSessions[sessionToken];
                    _activeSessions.Remove(sessionToken);
                    AuditLogger.LogAuthenticationEvent(_logger, "LOGOUT", session.Username, true);
                    _logger.LogInformation("User {Username} logged out", session.Username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for session: {SessionToken}", sessionToken);
            }
            
            return Task.CompletedTask;
        }

        public Task<List<TechnicianRole>> GetUserRolesAsync(string username)
        {
            try
            {
                // In a real implementation, this would query Active Directory or a database
                // For now, we'll simulate role assignment based on username patterns
                var roles = new List<TechnicianRole>();

                if (username.ToLower().Contains("admin") || username.ToLower().Contains("administrator"))
                {
                    roles.Add(_roles.First(r => r.Name == "Administrator"));
                }
                else if (username.ToLower().Contains("tech") || username.ToLower().Contains("support"))
                {
                    roles.Add(_roles.First(r => r.Name == "Technician"));
                }
                else
                {
                    // Default to basic technician role
                    roles.Add(_roles.First(r => r.Name == "Technician"));
                }

                return Task.FromResult(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {Username}", username);
                return Task.FromResult(new List<TechnicianRole>());
            }
        }

        public async Task<bool> HasPermissionAsync(string username, string permission)
        {
            try
            {
                var userRoles = await GetUserRolesAsync(username);
                return userRoles.Any(role => role.Permissions.Any(p => p.Name == permission));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user: {Username}", permission, username);
                return false;
            }
        }

        private Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            try
            {
                // Try Active Directory first
                using var context = new PrincipalContext(ContextType.Domain);
                using var user = UserPrincipal.FindByIdentity(context, username);
                
                if (user != null)
                {
                    return Task.FromResult(context.ValidateCredentials(username, password));
                }

                // Fallback to local machine
                using var localContext = new PrincipalContext(ContextType.Machine);
                using var localUser = UserPrincipal.FindByIdentity(localContext, username);
                
                if (localUser != null)
                {
                    return Task.FromResult(localContext.ValidateCredentials(username, password));
                }

                // Default test credentials for development/testing
                if (IsDefaultTestCredentials(username, password))
                {
                    _logger.LogWarning("Using default test credentials for user: {Username}", username);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch
            {
                // If AD is not available, try local validation first
                try
                {
                    using var context = new PrincipalContext(ContextType.Machine);
                    if (context.ValidateCredentials(username, password))
                    {
                        return Task.FromResult(true);
                    }
                }
                catch
                {
                    // Local validation failed, continue to default credentials
                }

                // Finally, check default test credentials
                if (IsDefaultTestCredentials(username, password))
                {
                    _logger.LogWarning("Using default test credentials for user: {Username} (AD/Local validation failed)", username);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        private string GenerateSessionToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private bool IsDefaultTestCredentials(string username, string password)
        {
            // Default test credentials for development/testing
            var defaultCredentials = new Dictionary<string, string>
            {
                { "admin", "admin123" },
                { "administrator", "admin123" },
                { "tech", "tech123" },
                { "technician", "tech123" },
                { "demo", "demo123" },
                { "test", "test123" }
            };

            return defaultCredentials.ContainsKey(username.ToLower()) && 
                   defaultCredentials[username.ToLower()] == password;
        }

        private List<TechnicianRole> InitializeRoles()
        {
            return new List<TechnicianRole>
            {
                new TechnicianRole
                {
                    Name = "Administrator",
                    Description = "Full system access",
                    Permissions = _permissions
                },
                new TechnicianRole
                {
                    Name = "Technician",
                    Description = "Standard technician access",
                    Permissions = _permissions.Where(p => p.Name != "AD_USER_MANAGEMENT" && p.Name != "SYSTEM_CONFIGURATION").ToList()
                },
                new TechnicianRole
                {
                    Name = "ReadOnly",
                    Description = "Read-only access",
                    Permissions = _permissions.Where(p => p.Name.Contains("VIEW") || p.Name.Contains("READ")).ToList()
                }
            };
        }

        private List<Permission> InitializePermissions()
        {
            return new List<Permission>
            {
                new Permission { Name = "NETWORK_TOOLS", Description = "Access network diagnostic tools" },
                new Permission { Name = "SYSTEM_HEALTH", Description = "View system health information" },
                new Permission { Name = "SECURITY_VIEW", Description = "View security status" },
                new Permission { Name = "AD_USER_MANAGEMENT", Description = "Manage Active Directory users" },
                new Permission { Name = "SYSTEM_CONFIGURATION", Description = "Modify system configuration" },
                new Permission { Name = "AUTOMATION_TOOLS", Description = "Run automation scripts" },
                new Permission { Name = "TROUBLESHOOTING", Description = "Access troubleshooting tools" },
                new Permission { Name = "REPORTING", Description = "Generate reports" },
                new Permission { Name = "WINDOWS11_TOOLS", Description = "Access Windows 11 upgrade tools" }
            };
        }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? SessionToken { get; set; }
        public string? Username { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime? ExpiresAt { get; set; }
        public string? Error { get; set; }
    }

    public class UserSession
    {
        public string Username { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime LastActivity { get; set; }
        public List<TechnicianRole> Roles { get; set; } = new();
    }

    public class TechnicianRole
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Permission> Permissions { get; set; } = new();
    }

    public class Permission
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
