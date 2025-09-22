using Microsoft.Extensions.Logging;
using System.Security.Principal;

namespace EnterpriseITToolkit.Security
{
    public static class AuditLogger
    {
        public static void LogSecurityEvent(ILogger logger, string action, string details, string? user = null)
        {
            user ??= GetCurrentUser();
            
            logger.LogWarning("SECURITY_AUDIT: User={User}, Action={Action}, Details={Details}, Timestamp={Timestamp}",
                user, action, details, DateTime.UtcNow);
        }

        public static void LogAuthenticationEvent(ILogger logger, string eventType, string user, bool success, string? details = null)
        {
            var level = success ? LogLevel.Information : LogLevel.Warning;
            
            logger.Log(level, "AUTH_EVENT: Type={EventType}, User={User}, Success={Success}, Details={Details}, Timestamp={Timestamp}",
                eventType, user, success, details, DateTime.UtcNow);
        }

        public static void LogCommandExecution(ILogger logger, string command, string[] args, bool success, string? user = null)
        {
            user ??= GetCurrentUser();
            
            var level = success ? LogLevel.Information : LogLevel.Warning;
            
            logger.Log(level, "COMMAND_EXECUTION: User={User}, Command={Command}, Args={Args}, Success={Success}, Timestamp={Timestamp}",
                user, command, string.Join(" ", args), success, DateTime.UtcNow);
        }

        public static void LogNetworkAccess(ILogger logger, string target, string operation, bool success, string? user = null)
        {
            user ??= GetCurrentUser();
            
            var level = success ? LogLevel.Information : LogLevel.Warning;
            
            logger.Log(level, "NETWORK_ACCESS: User={User}, Target={Target}, Operation={Operation}, Success={Success}, Timestamp={Timestamp}",
                user, target, operation, success, DateTime.UtcNow);
        }

        public static void LogSystemAccess(ILogger logger, string operation, string details, bool success, string? user = null)
        {
            user ??= GetCurrentUser();
            
            var level = success ? LogLevel.Information : LogLevel.Warning;
            
            logger.Log(level, "SYSTEM_ACCESS: User={User}, Operation={Operation}, Details={Details}, Success={Success}, Timestamp={Timestamp}",
                user, operation, details, success, DateTime.UtcNow);
        }

        private static string GetCurrentUser()
        {
            try
            {
                return WindowsIdentity.GetCurrent().Name ?? Environment.UserName;
            }
            catch
            {
                return Environment.UserName;
            }
        }
    }
}
