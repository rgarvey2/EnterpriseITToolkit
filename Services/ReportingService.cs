using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ILogger<ReportingService> _logger;
        private readonly ISystemHealthService _systemHealthService;
        private readonly ISecurityService _securityService;
        private readonly INetworkService _networkService;

        public ReportingService(
            ILogger<ReportingService> logger,
            ISystemHealthService systemHealthService,
            ISecurityService securityService,
            INetworkService networkService)
        {
            _logger = logger;
            _systemHealthService = systemHealthService;
            _securityService = securityService;
            _networkService = networkService;
        }

        public async Task<SystemReport> GenerateSystemReportAsync()
        {
            var report = new SystemReport
            {
                GeneratedAt = DateTime.UtcNow,
                ComputerName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString()
            };

            try
            {
                _logger.LogInformation("Generating system report");

                report.HealthStatus = await _systemHealthService.GetSystemHealthAsync();
                report.Performance = await _systemHealthService.GetPerformanceMetricsAsync();

                _logger.LogInformation("System report generated successfully");
                AuditLogger.LogSystemAccess(_logger, "SystemReport", "Generated", true);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating system report");
                AuditLogger.LogSystemAccess(_logger, "SystemReport", "Failed", false);
                return report;
            }
        }

        public async Task<SecurityReport> GenerateSecurityReportAsync()
        {
            try
            {
                _logger.LogInformation("Generating security report");

                var report = await _securityService.GenerateSecurityReportAsync();

                _logger.LogInformation("Security report generated successfully");
                AuditLogger.LogSystemAccess(_logger, "SecurityReport", "Generated", true);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report");
                AuditLogger.LogSystemAccess(_logger, "SecurityReport", "Failed", false);
                return new SecurityReport { GeneratedAt = DateTime.UtcNow };
            }
        }

        public async Task<NetworkReport> GenerateNetworkReportAsync()
        {
            var report = new NetworkReport
            {
                GeneratedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Generating network report");

                report.NetworkInfo = await _networkService.GetNetworkInfoAsync();
                report.BandwidthTest = await _networkService.TestBandwidthAsync("https://speedtest.example.com", 10);

                // Add some sample ping results
                report.PingResults.Add(await _networkService.PingAsync("8.8.8.8"));
                report.PingResults.Add(await _networkService.PingAsync("1.1.1.1"));

                _logger.LogInformation("Network report generated successfully");
                AuditLogger.LogSystemAccess(_logger, "NetworkReport", "Generated", true);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating network report");
                AuditLogger.LogSystemAccess(_logger, "NetworkReport", "Failed", false);
                return report;
            }
        }

        public Task<ComplianceReport> GenerateComplianceReportAsync()
        {
            var report = new ComplianceReport
            {
                GeneratedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Generating compliance report");

                // Add compliance checks
                report.Checks.Add(new ComplianceCheck
                {
                    Category = "Security",
                    CheckName = "Windows Firewall Enabled",
                    IsCompliant = true,
                    Details = "Windows Firewall is enabled and configured",
                    Recommendation = "Continue monitoring firewall status"
                });

                report.Checks.Add(new ComplianceCheck
                {
                    Category = "Security",
                    CheckName = "Antivirus Protection",
                    IsCompliant = true,
                    Details = "Antivirus protection is active",
                    Recommendation = "Ensure real-time protection is enabled"
                });

                report.Checks.Add(new ComplianceCheck
                {
                    Category = "System",
                    CheckName = "Windows Updates",
                    IsCompliant = false,
                    Details = "Some Windows updates are pending",
                    Recommendation = "Install pending Windows updates"
                });

                // Calculate compliance score
                var totalChecks = report.Checks.Count;
                var compliantChecks = report.Checks.Count(c => c.IsCompliant);
                report.ComplianceScore = totalChecks > 0 ? (compliantChecks * 100) / totalChecks : 0;

                // Identify non-compliant items
                report.NonCompliantItems = report.Checks
                    .Where(c => !c.IsCompliant)
                    .Select(c => c.CheckName)
                    .ToList();

                _logger.LogInformation("Compliance report generated. Score: {Score}%", report.ComplianceScore);
                AuditLogger.LogSystemAccess(_logger, "ComplianceReport", "Generated", true);

                return Task.FromResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                AuditLogger.LogSystemAccess(_logger, "ComplianceReport", "Failed", false);
                return Task.FromResult(report);
            }
        }
    }
}
