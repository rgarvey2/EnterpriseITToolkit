using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public class SiemService : ISiemService
    {
        private readonly ILogger<SiemService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private SiemConfiguration? _siemConfig;

        public SiemService(ILogger<SiemService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _siemConfig = LoadSiemConfiguration();
        }

        public async Task<bool> SendSecurityEventAsync(SecurityEvent securityEvent)
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                _logger.LogWarning("SIEM integration is disabled or not configured");
                return false;
            }

            try
            {
                var payload = CreateSecurityEventPayload(securityEvent);
                var success = await SendToSiemAsync(payload, "security-events");
                
                if (success)
                {
                    _logger.LogInformation("Security event sent to SIEM: {EventType}", securityEvent.EventType);
                }
                else
                {
                    _logger.LogWarning("Failed to send security event to SIEM: {EventType}", securityEvent.EventType);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security event to SIEM: {EventType}", securityEvent.EventType);
                return false;
            }
        }

        public async Task<bool> SendAuditLogAsync(AuditLog auditLog)
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                _logger.LogWarning("SIEM integration is disabled or not configured");
                return false;
            }

            try
            {
                var payload = CreateAuditLogPayload(auditLog);
                var success = await SendToSiemAsync(payload, "audit-logs");
                
                if (success)
                {
                    _logger.LogInformation("Audit log sent to SIEM: {Action}", auditLog.Action);
                }
                else
                {
                    _logger.LogWarning("Failed to send audit log to SIEM: {Action}", auditLog.Action);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending audit log to SIEM: {Action}", auditLog.Action);
                return false;
            }
        }

        public Task<List<SecurityEvent>> GetSecurityEventsFromSiemAsync(DateTime from, DateTime to)
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                _logger.LogWarning("SIEM integration is disabled or not configured");
                return Task.FromResult(new List<SecurityEvent>());
            }

            try
            {
                var query = $"search index=security earliest={from:yyyy-MM-ddTHH:mm:ss} latest={to:yyyy-MM-ddTHH:mm:ss}";
                var response = QuerySiemAsync(query).Result;
                
                if (response != null)
                {
                    return Task.FromResult(ParseSecurityEventsFromSiem(response));
                }
                
                return Task.FromResult(new List<SecurityEvent>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security events from SIEM");
                return Task.FromResult(new List<SecurityEvent>());
            }
        }

        public Task<List<AuditLog>> GetAuditLogsFromSiemAsync(DateTime from, DateTime to)
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                _logger.LogWarning("SIEM integration is disabled or not configured");
                return Task.FromResult(new List<AuditLog>());
            }

            try
            {
                var query = $"search index=audit earliest={from:yyyy-MM-ddTHH:mm:ss} latest={to:yyyy-MM-ddTHH:mm:ss}";
                var response = QuerySiemAsync(query).Result;
                
                if (response != null)
                {
                    return Task.FromResult(ParseAuditLogsFromSiem(response));
                }
                
                return Task.FromResult(new List<AuditLog>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs from SIEM");
                return Task.FromResult(new List<AuditLog>());
            }
        }

        public Task<bool> TestSiemConnectionAsync()
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                return Task.FromResult(false);
            }

            try
            {
                var testQuery = "search index=_internal | head 1";
                var response = QuerySiemAsync(testQuery).Result;
                return Task.FromResult(response != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SIEM connection test failed");
                return Task.FromResult(false);
            }
        }

        public Task<SiemConfiguration> GetSiemConfigurationAsync()
        {
            return Task.FromResult(_siemConfig ?? new SiemConfiguration());
        }

        public Task<bool> UpdateSiemConfigurationAsync(SiemConfiguration configuration)
        {
            try
            {
                _siemConfig = configuration;
                SaveSiemConfigurationAsync(configuration).Wait();
                _logger.LogInformation("SIEM configuration updated successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SIEM configuration");
                return Task.FromResult(false);
            }
        }

        public Task<List<ThreatIntelligence>> GetThreatIntelligenceAsync()
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                return Task.FromResult(new List<ThreatIntelligence>());
            }

            try
            {
                // This would typically query a threat intelligence feed
                // For now, return some sample data
                return Task.FromResult(new List<ThreatIntelligence>
                {
                    new ThreatIntelligence
                    {
                        Id = 1,
                        ThreatType = "Malware",
                        ThreatName = "Sample Ransomware",
                        Description = "Known ransomware variant",
                        Severity = "High",
                        Source = "Threat Intelligence Feed",
                        Indicators = new List<string> { "192.168.1.100", "malicious-domain.com" },
                        FirstSeen = DateTime.UtcNow.AddDays(-7),
                        LastSeen = DateTime.UtcNow,
                        IsActive = true,
                        Mitigation = "Block IP addresses and domains"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving threat intelligence");
                return Task.FromResult(new List<ThreatIntelligence>());
            }
        }

        public Task<bool> SendThreatIntelligenceAsync(ThreatIntelligence threat)
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                return Task.FromResult(false);
            }

            try
            {
                var payload = CreateThreatIntelligencePayload(threat);
                return Task.FromResult(SendToSiemAsync(payload, "threat-intelligence").Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending threat intelligence to SIEM");
                return Task.FromResult(false);
            }
        }

        public Task<List<SecurityAlert>> GetSecurityAlertsAsync()
        {
            if (_siemConfig == null || !_siemConfig.IsEnabled)
            {
                return Task.FromResult(new List<SecurityAlert>());
            }

            try
            {
                // This would typically query the SIEM for active alerts
                // For now, return some sample data
                return Task.FromResult(new List<SecurityAlert>
                {
                    new SecurityAlert
                    {
                        Id = 1,
                        AlertType = "Failed Login Attempts",
                        Title = "Multiple Failed Login Attempts Detected",
                        Description = "Detected 5 failed login attempts from IP 192.168.1.50",
                        Severity = "Medium",
                        Status = "New",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                        AffectedSystems = new List<string> { "DC01", "FileServer01" },
                        Source = "SIEM"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security alerts");
                return Task.FromResult(new List<SecurityAlert>());
            }
        }

        public Task<bool> AcknowledgeSecurityAlertAsync(int alertId, string acknowledgedBy)
        {
            try
            {
                // This would typically update the alert in the SIEM
                _logger.LogInformation("Security alert {AlertId} acknowledged by {User}", alertId, acknowledgedBy);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging security alert {AlertId}", alertId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ResolveSecurityAlertAsync(int alertId, string resolvedBy, string resolution)
        {
            try
            {
                // This would typically update the alert in the SIEM
                _logger.LogInformation("Security alert {AlertId} resolved by {User}: {Resolution}", alertId, resolvedBy, resolution);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving security alert {AlertId}", alertId);
                return Task.FromResult(false);
            }
        }

        private async Task<bool> SendToSiemAsync(object payload, string endpoint)
        {
            if (_siemConfig == null) return false;

            try
            {
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add authentication headers
                if (!string.IsNullOrEmpty(_siemConfig.ApiKey))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _siemConfig.ApiKey);
                }

                var url = $"{_siemConfig.Endpoint}/{endpoint}";
                var response = await _httpClient.PostAsync(url, content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data to SIEM endpoint: {Endpoint}", endpoint);
                return false;
            }
        }

        private async Task<string?> QuerySiemAsync(string query)
        {
            if (_siemConfig == null) return null;

            try
            {
                var payload = new { query = query };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_siemConfig.Endpoint}/search";
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying SIEM: {Query}", query);
                return null;
            }
        }

        private object CreateSecurityEventPayload(SecurityEvent securityEvent)
        {
            return new
            {
                timestamp = securityEvent.Timestamp,
                event_type = securityEvent.EventType,
                severity = securityEvent.Severity,
                user_id = securityEvent.UserId,
                details = securityEvent.Description,
                ip_address = securityEvent.IpAddress,
                source = "EnterpriseITToolkit"
            };
        }

        private object CreateAuditLogPayload(AuditLog auditLog)
        {
            return new
            {
                timestamp = auditLog.Timestamp,
                action = auditLog.Action,
                user_id = auditLog.UserId,
                resource = auditLog.Resource,
                details = auditLog.Details,
                success = auditLog.Success,
                ip_address = auditLog.IpAddress,
                source = "EnterpriseITToolkit"
            };
        }

        private object CreateThreatIntelligencePayload(ThreatIntelligence threat)
        {
            return new
            {
                threat_type = threat.ThreatType,
                threat_name = threat.ThreatName,
                description = threat.Description,
                severity = threat.Severity,
                indicators = threat.Indicators,
                first_seen = threat.FirstSeen,
                last_seen = threat.LastSeen,
                is_active = threat.IsActive,
                mitigation = threat.Mitigation,
                source = "EnterpriseITToolkit"
            };
        }

        private List<SecurityEvent> ParseSecurityEventsFromSiem(string response)
        {
            // This would parse the SIEM response and convert to SecurityEvent objects
            // For now, return empty list
            return new List<SecurityEvent>();
        }

        private List<AuditLog> ParseAuditLogsFromSiem(string response)
        {
            // This would parse the SIEM response and convert to AuditLog objects
            // For now, return empty list
            return new List<AuditLog>();
        }

        private SiemConfiguration LoadSiemConfiguration()
        {
            try
            {
                var config = new SiemConfiguration();
                _configuration.GetSection("SiemConfiguration").Bind(config);
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SIEM configuration");
                return new SiemConfiguration { IsEnabled = false };
            }
        }

        private Task SaveSiemConfigurationAsync(SiemConfiguration configuration)
        {
            // This would typically save to a configuration file or database
            // For now, just log the configuration update
            _logger.LogInformation("SIEM configuration saved");
            return Task.CompletedTask;
        }
    }
}
