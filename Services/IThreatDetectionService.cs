using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public interface IThreatDetectionService
    {
        Task<List<ThreatDetection>> AnalyzeSecurityEventsAsync(List<SecurityEvent> events);
        Task<List<ThreatDetection>> AnalyzeAuditLogsAsync(List<AuditLog> logs);
        Task<ThreatDetection> DetectAnomalousLoginAsync(string username, string ipAddress, DateTime timestamp);
        Task<ThreatDetection?> DetectBruteForceAttackAsync(string target, List<SecurityEvent> events);
        Task<ThreatDetection?> DetectPrivilegeEscalationAsync(List<AuditLog> logs);
        Task<ThreatDetection?> DetectDataExfiltrationAsync(List<AuditLog> logs);
        Task<ThreatDetection?> DetectMalwareActivityAsync(List<SecurityEvent> events);
        Task<List<ThreatDetection>> GetActiveThreatsAsync();
        Task<bool> UpdateThreatDetectionRulesAsync(List<ThreatDetectionRule> rules);
        Task<List<ThreatDetectionRule>> GetThreatDetectionRulesAsync();
        Task<ThreatDetection> CreateThreatDetectionAsync(ThreatDetection threat);
        Task<bool> UpdateThreatDetectionStatusAsync(int threatId, ThreatStatus status, string updatedBy);
    }

    public class ThreatDetection
    {
        public int Id { get; set; }
        public string ThreatType { get; set; } = string.Empty; // "BruteForce", "PrivilegeEscalation", "DataExfiltration", "Malware", "AnomalousLogin"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
        public ThreatStatus Status { get; set; } = ThreatStatus.New;
        public string Confidence { get; set; } = string.Empty; // "Low", "Medium", "High"
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? Resolution { get; set; }
        public List<string> AffectedSystems { get; set; } = new();
        public List<string> Indicators { get; set; } = new();
        public Dictionary<string, object> Evidence { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
        public string Source { get; set; } = string.Empty;
        public bool IsFalsePositive { get; set; } = false;
        public string? FalsePositiveReason { get; set; }
    }

    public enum ThreatStatus
    {
        New,
        Investigating,
        Confirmed,
        Mitigated,
        Resolved,
        FalsePositive
    }

    public class ThreatDetectionRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThreatType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public string Condition { get; set; } = string.Empty; // JSON or expression
        public int Threshold { get; set; } = 1;
        public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
        public List<string> Actions { get; set; } = new(); // "Alert", "Block", "Log", "Quarantine"
        public DateTime CreatedAt { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; } = 0;
    }
}
