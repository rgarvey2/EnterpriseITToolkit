using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public interface ISiemService
    {
        Task<bool> SendSecurityEventAsync(SecurityEvent securityEvent);
        Task<bool> SendAuditLogAsync(AuditLog auditLog);
        Task<List<SecurityEvent>> GetSecurityEventsFromSiemAsync(DateTime from, DateTime to);
        Task<List<AuditLog>> GetAuditLogsFromSiemAsync(DateTime from, DateTime to);
        Task<bool> TestSiemConnectionAsync();
        Task<SiemConfiguration> GetSiemConfigurationAsync();
        Task<bool> UpdateSiemConfigurationAsync(SiemConfiguration configuration);
        Task<List<ThreatIntelligence>> GetThreatIntelligenceAsync();
        Task<bool> SendThreatIntelligenceAsync(ThreatIntelligence threat);
        Task<List<SecurityAlert>> GetSecurityAlertsAsync();
        Task<bool> AcknowledgeSecurityAlertAsync(int alertId, string acknowledgedBy);
        Task<bool> ResolveSecurityAlertAsync(int alertId, string resolvedBy, string resolution);
    }

    public class SiemConfiguration
    {
        public string SiemType { get; set; } = string.Empty; // "Splunk", "ELK", "Azure Sentinel", "QRadar"
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public int BatchSize { get; set; } = 100;
        public int RetryAttempts { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 30;
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
        public List<string> EnabledEventTypes { get; set; } = new();
    }

    public class ThreatIntelligence
    {
        public int Id { get; set; }
        public string ThreatType { get; set; } = string.Empty; // "Malware", "Phishing", "DDoS", "APT"
        public string ThreatName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public string Source { get; set; } = string.Empty;
        public List<string> Indicators { get; set; } = new(); // IPs, URLs, File hashes, etc.
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsActive { get; set; } = true;
        public string Mitigation { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class SecurityAlert
    {
        public int Id { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "New", "Acknowledged", "Investigating", "Resolved"
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
        public string? ResolvedBy { get; set; }
        public string? Resolution { get; set; }
        public List<string> AffectedSystems { get; set; } = new();
        public Dictionary<string, object> AlertData { get; set; } = new();
        public string Source { get; set; } = string.Empty;
    }
}
