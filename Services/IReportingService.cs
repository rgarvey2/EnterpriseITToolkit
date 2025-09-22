namespace EnterpriseITToolkit.Services
{
    public interface IReportingService
    {
        Task<SystemReport> GenerateSystemReportAsync();
        Task<SecurityReport> GenerateSecurityReportAsync();
        Task<NetworkReport> GenerateNetworkReportAsync();
        Task<ComplianceReport> GenerateComplianceReportAsync();
    }

    public class SystemReport
    {
        public DateTime GeneratedAt { get; set; }
        public string ComputerName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public SystemHealthResult HealthStatus { get; set; } = new();
        public PerformanceMetrics Performance { get; set; } = new();
        public List<SystemIssue> Issues { get; set; } = new();
    }

    public class NetworkReport
    {
        public DateTime GeneratedAt { get; set; }
        public NetworkInfo NetworkInfo { get; set; } = new();
        public List<PingResult> PingResults { get; set; } = new();
        public List<PortScanResult> PortScanResults { get; set; } = new();
        public BandwidthTestResult BandwidthTest { get; set; } = new();
    }

    public class ComplianceReport
    {
        public DateTime GeneratedAt { get; set; }
        public List<ComplianceCheck> Checks { get; set; } = new();
        public int ComplianceScore { get; set; }
        public List<string> NonCompliantItems { get; set; } = new();
    }

    public class ComplianceCheck
    {
        public string Category { get; set; } = string.Empty;
        public string CheckName { get; set; } = string.Empty;
        public bool IsCompliant { get; set; }
        public string Details { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }
}
