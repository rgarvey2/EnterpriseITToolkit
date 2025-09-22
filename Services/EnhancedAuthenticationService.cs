using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;
using EnterpriseITToolkit.Data;
using EnterpriseITToolkit.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace EnterpriseITToolkit.Services
{
    public interface IEnhancedAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? mfaCode = null);
        Task<bool> ValidateSessionAsync(string sessionToken);
        Task LogoutAsync(string sessionToken);
        Task<List<Role>> GetUserRolesAsync(string username);
        Task<bool> HasPermissionAsync(string username, string permission);
        Task<bool> EnableMfaAsync(string username);
        Task<bool> DisableMfaAsync(string username);
        Task<string> GenerateMfaSecretAsync(string username);
        Task<bool> ValidateMfaCodeAsync(string username, string code);
        Task<string> GenerateQrCodeAsync(string username, string secret);
        Task<SessionInfo> GetSessionInfoAsync(string sessionToken);
        Task<List<UserSession>> GetActiveSessionsAsync(string username);
        Task<bool> TerminateSessionAsync(string sessionToken);
        Task<bool> TerminateAllSessionsAsync(string username);
    }

    public class EnhancedAuthenticationService : IEnhancedAuthenticationService
    {
        private readonly ILogger<EnhancedAuthenticationService> _logger;
        private readonly EnterpriseDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly ITotpService _totpService;
        private readonly Dictionary<string, Models.UserSession> _activeSessions;

        public EnhancedAuthenticationService(
            ILogger<EnhancedAuthenticationService> logger,
            EnterpriseDbContext context,
            IConfiguration configuration,
            IAuditService auditService,
            ITotpService totpService)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _auditService = auditService;
            _totpService = totpService;
            _activeSessions = new Dictionary<string, UserSession>();
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? mfaCode = null)
        {
            try
            {
                _logger.LogInformation("Authentication attempt for user: {Username}", username);

                // Validate input
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    await _auditService.LogSecurityEventAsync("LOGIN_ATTEMPT", "FAILED", username, "Empty credentials");
                    return new AuthenticationResult { Success = false, Error = "Invalid credentials" };
                }

                // Get user from database
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null || !user.IsActive || user.IsLocked)
                {
                    await _auditService.LogSecurityEventAsync("LOGIN_ATTEMPT", "FAILED", username, "User not found or inactive");
                    return new AuthenticationResult { Success = false, Error = "Invalid username or password" };
                }

                // Validate credentials
                bool isValidCredentials = await ValidateUserCredentialsAsync(username, password);
                if (!isValidCredentials)
                {
                    await _auditService.LogSecurityEventAsync("LOGIN_ATTEMPT", "FAILED", username, "Invalid credentials");
                    return new AuthenticationResult { Success = false, Error = "Invalid username or password" };
                }

                // Check MFA if enabled
                if (user.MfaEnabled && string.IsNullOrEmpty(mfaCode))
                {
                    await _auditService.LogSecurityEventAsync("LOGIN_ATTEMPT", "MFA_REQUIRED", username, "MFA code required");
                    return new AuthenticationResult { Success = false, Error = "MFA code required", RequiresMfa = true };
                }

                if (user.MfaEnabled && !string.IsNullOrEmpty(mfaCode))
                {
                    bool isValidMfa = await ValidateMfaCodeAsync(username, mfaCode);
                    if (!isValidMfa)
                    {
                        await _auditService.LogSecurityEventAsync("LOGIN_ATTEMPT", "FAILED", username, "Invalid MFA code");
                        return new AuthenticationResult { Success = false, Error = "Invalid MFA code" };
                    }
                }

                // Check if user has required roles
                var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();
                if (!userRoles.Any(r => r.Name == "Technician" || r.Name == "Administrator"))
                {
                    await _auditService.LogSecurityEventAsync("LOGIN_ATTEMPT", "FAILED", username, "Insufficient privileges");
                    return new AuthenticationResult { Success = false, Error = "Access denied. Technician privileges required." };
                }

                // Create session
                var sessionToken = GenerateSessionToken();
                var session = new Models.UserSession
                {
                    Username = username,
                    SessionToken = sessionToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(8), // 8-hour session
                    Roles = userRoles.Select(r => r.Name).ToList(),
                    LastActivity = DateTime.UtcNow,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent()
                };

                _activeSessions[sessionToken] = session;

                // Update user last login
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditService.LogSecurityEventAsync("LOGIN_SUCCESS", "SUCCESS", username, "User authenticated successfully");
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
                await _auditService.LogSecurityEventAsync("LOGIN_ERROR", "ERROR", username, ex.Message);
                return new AuthenticationResult { Success = false, Error = "Authentication service error" };
            }
        }

        public async Task<bool> ValidateSessionAsync(string sessionToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionToken) || !_activeSessions.ContainsKey(sessionToken))
                {
                    return false;
                }

                var session = _activeSessions[sessionToken];
                
                // Check if session is expired
                if (session.ExpiresAt < DateTime.UtcNow)
                {
                    _activeSessions.Remove(sessionToken);
                    await _auditService.LogSecurityEventAsync("SESSION_EXPIRED", "INFO", session.Username, "Session expired");
                    return false;
                }

                // Update last activity
                session.LastActivity = DateTime.UtcNow;
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session: {SessionToken}", sessionToken);
                return false;
            }
        }

        public async Task LogoutAsync(string sessionToken)
        {
            try
            {
                if (_activeSessions.ContainsKey(sessionToken))
                {
                    var session = _activeSessions[sessionToken];
                    _activeSessions.Remove(sessionToken);
                    await _auditService.LogSecurityEventAsync("LOGOUT", "SUCCESS", session.Username, "User logged out");
                    _logger.LogInformation("User {Username} logged out", session.Username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for session: {SessionToken}", sessionToken);
            }
        }

        public async Task<List<Role>> GetUserRolesAsync(string username)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == username);

                return user?.UserRoles.Select(ur => ur.Role).ToList() ?? new List<Role>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {Username}", username);
                return new List<Role>();
            }
        }

        public async Task<bool> HasPermissionAsync(string username, string permission)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null) return false;

                return user.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Any(rp => rp.Permission.Name == permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user: {Username}", permission, username);
                return false;
            }
        }

        public async Task<bool> EnableMfaAsync(string username)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                user.MfaEnabled = true;
                user.MfaSecret = await _totpService.GenerateSecretAsync();
                await _context.SaveChangesAsync();

                await _auditService.LogSecurityEventAsync("MFA_ENABLED", "SUCCESS", username, "MFA enabled for user");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling MFA for user: {Username}", username);
                return false;
            }
        }

        public async Task<bool> DisableMfaAsync(string username)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return false;

                user.MfaEnabled = false;
                user.MfaSecret = null;
                await _context.SaveChangesAsync();

                await _auditService.LogSecurityEventAsync("MFA_DISABLED", "SUCCESS", username, "MFA disabled for user");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA for user: {Username}", username);
                return false;
            }
        }

        public async Task<string> GenerateMfaSecretAsync(string username)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return string.Empty;

                var secret = await _totpService.GenerateSecretAsync();
                user.MfaSecret = secret;
                await _context.SaveChangesAsync();

                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating MFA secret for user: {Username}", username);
                return string.Empty;
            }
        }

        public async Task<bool> ValidateMfaCodeAsync(string username, string code)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null || string.IsNullOrEmpty(user.MfaSecret)) return false;

                return await _totpService.ValidateCodeAsync(user.MfaSecret, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MFA code for user: {Username}", username);
                return false;
            }
        }

        public async Task<string> GenerateQrCodeAsync(string username, string secret)
        {
            try
            {
                return await _totpService.GenerateQrCodeAsync(username, secret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for user: {Username}", username);
                return string.Empty;
            }
        }

        public Task<SessionInfo> GetSessionInfoAsync(string sessionToken)
        {
            try
            {
                if (!_activeSessions.ContainsKey(sessionToken))
                {
                    return Task.FromResult(new SessionInfo { IsValid = false });
                }

                var session = _activeSessions[sessionToken];
                return Task.FromResult(new SessionInfo
                {
                    IsValid = true,
                    Username = session.Username,
                    Roles = session.Roles,
                    CreatedAt = session.CreatedAt,
                    ExpiresAt = session.ExpiresAt,
                    LastActivity = session.LastActivity,
                    IpAddress = session.IpAddress
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session info for token: {SessionToken}", sessionToken);
                return Task.FromResult(new SessionInfo { IsValid = false });
            }
        }

        public Task<List<Models.UserSession>> GetActiveSessionsAsync(string username)
        {
            try
            {
                return Task.FromResult(_activeSessions.Values
                    .Where(s => s.Username == username)
                    .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions for user: {Username}", username);
                return Task.FromResult(new List<Models.UserSession>());
            }
        }

        public async Task<bool> TerminateSessionAsync(string sessionToken)
        {
            try
            {
                if (_activeSessions.ContainsKey(sessionToken))
                {
                    var session = _activeSessions[sessionToken];
                    _activeSessions.Remove(sessionToken);
                    await _auditService.LogSecurityEventAsync("SESSION_TERMINATED", "SUCCESS", session.Username, "Session terminated");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating session: {SessionToken}", sessionToken);
                return false;
            }
        }

        public Task<bool> TerminateAllSessionsAsync(string username)
        {
            try
            {
                var sessionsToRemove = _activeSessions.Values
                    .Where(s => s.Username == username)
                    .ToList();

                foreach (var session in sessionsToRemove)
                {
                    _activeSessions.Remove(session.SessionToken);
                }

                _ = _auditService.LogSecurityEventAsync("ALL_SESSIONS_TERMINATED", "SUCCESS", username, "All sessions terminated");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error terminating all sessions for user: {Username}", username);
                return Task.FromResult(false);
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

        private string? GetClientIpAddress()
        {
            // This would be implemented based on your application's context
            // For now, return a placeholder
            return "127.0.0.1";
        }

        private string? GetUserAgent()
        {
            // This would be implemented based on your application's context
            // For now, return a placeholder
            return "EnterpriseITToolkit/4.0";
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
        public bool RequiresMfa { get; set; }
    }

    public class SessionInfo
    {
        public bool IsValid { get; set; }
        public string? Username { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime LastActivity { get; set; }
        public string? IpAddress { get; set; }
    }
}
