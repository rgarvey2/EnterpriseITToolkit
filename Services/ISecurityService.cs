namespace EnterpriseITToolkit.Services
{
    public interface ISecurityService
    {
        Task<FirewallStatus> GetFirewallStatusAsync();
        Task<AntivirusStatus> GetAntivirusStatusAsync();
        Task<SecurityReport> GenerateSecurityReportAsync();
    }

    public class FirewallStatus
    {
        public bool IsEnabled { get; set; }
        public string DomainProfile { get; set; } = string.Empty;
        public string PrivateProfile { get; set; } = string.Empty;
        public string PublicProfile { get; set; } = string.Empty;
        public List<FirewallRule> Rules { get; set; } = new();
    }

    public class FirewallRule
    {
        public string Name { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string LocalPort { get; set; } = string.Empty;
        public string RemoteAddress { get; set; } = string.Empty;
    }

    public class AntivirusStatus
    {
        public string ProductName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool RealTimeProtection { get; set; }
        public bool CloudProtection { get; set; }
        public DateTime LastScan { get; set; }
        public string Version { get; set; } = string.Empty;
    }

    public class SecurityReport
    {
        public DateTime GeneratedAt { get; set; }
        public FirewallStatus Firewall { get; set; } = new();
        public AntivirusStatus Antivirus { get; set; } = new();
        public List<SecurityRecommendation> Recommendations { get; set; } = new();
        public int RiskScore { get; set; }
    }

    public class SecurityRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}
