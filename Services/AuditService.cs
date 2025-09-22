using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EnterpriseITToolkit.Data;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public interface IAuditService
    {
        Task LogSecurityEventAsync(string eventType, string severity, string? userId, string? details);
        Task LogAuditEventAsync(string action, string? userId, string? resource, string? details, bool success = true);
        Task LogSystemEventAsync(string eventType, string? computerName, string? details);
        Task<bool> HasPermissionAsync(string username, string permission);
        Task<List<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50, string? userId = null, string? action = null);
        Task<List<SecurityEvent>> GetSecurityEventsAsync(int page = 1, int pageSize = 50, string? eventType = null, string? severity = null);
        Task<bool> ArchiveOldLogsAsync(int daysToKeep = 90);
    }

    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly EnterpriseDbContext _context;

        public AuditService(ILogger<AuditService> logger, EnterpriseDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task LogSecurityEventAsync(string eventType, string severity, string? userId, string? details)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = eventType,
                    Severity = severity,
                    UserId = userId,
                    Description = details,
                    Timestamp = DateTime.UtcNow,
                    Source = "EnterpriseITToolkit"
                };

                _context.SecurityEvents.Add(securityEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Security event logged: {EventType} - {Severity} - {UserId} - {Details}", 
                    eventType, severity, userId, details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security event: {EventType}", eventType);
            }
        }

        public async Task LogAuditEventAsync(string action, string? userId, string? resource, string? details, bool success = true)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Resource = resource,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    Success = success
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit event logged: {Action} - {UserId} - {Resource} - {Success}", 
                    action, userId, resource, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit event: {Action}", action);
            }
        }

        public async Task LogSystemEventAsync(string eventType, string? computerName, string? details)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = eventType,
                    Severity = "INFO",
                    Description = details,
                    Timestamp = DateTime.UtcNow,
                    Source = computerName ?? "Unknown"
                };

                _context.SecurityEvents.Add(securityEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("System event logged: {EventType} - {ComputerName} - {Details}", 
                    eventType, computerName, details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging system event: {EventType}", eventType);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50, string? userId = null, string? action = null)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(a => a.UserId == userId);

                if (!string.IsNullOrEmpty(action))
                    query = query.Where(a => a.Action == action);

                return await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return new List<AuditLog>();
            }
        }

        public async Task<List<SecurityEvent>> GetSecurityEventsAsync(int page = 1, int pageSize = 50, string? eventType = null, string? severity = null)
        {
            try
            {
                var query = _context.SecurityEvents.AsQueryable();

                if (!string.IsNullOrEmpty(eventType))
                    query = query.Where(s => s.EventType == eventType);

                if (!string.IsNullOrEmpty(severity))
                    query = query.Where(s => s.Severity == severity);

                return await query
                    .OrderByDescending(s => s.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security events");
                return new List<SecurityEvent>();
            }
        }

        public async Task<bool> HasPermissionAsync(string username, string permission)
        {
            try
            {
                // This is a simplified implementation
                // In a real enterprise system, this would check against the user's roles and permissions
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

        public async Task<bool> ArchiveOldLogsAsync(int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

                // Archive old audit logs
                var oldAuditLogs = await _context.AuditLogs
                    .Where(a => a.Timestamp < cutoffDate)
                    .ToListAsync();

                if (oldAuditLogs.Any())
                {
                    _context.AuditLogs.RemoveRange(oldAuditLogs);
                    _logger.LogInformation("Archived {Count} old audit logs", oldAuditLogs.Count);
                }

                // Archive old security events
                var oldSecurityEvents = await _context.SecurityEvents
                    .Where(s => s.Timestamp < cutoffDate)
                    .ToListAsync();

                if (oldSecurityEvents.Any())
                {
                    _context.SecurityEvents.RemoveRange(oldSecurityEvents);
                    _logger.LogInformation("Archived {Count} old security events", oldSecurityEvents.Count);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving old logs");
                return false;
            }
        }
    }
}
