using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : BaseApiController
    {
        private readonly ISiemService _siemService;
        private readonly IThreatDetectionService _threatDetectionService;

        public SecurityController(
            ILogger<SecurityController> logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService,
            ISiemService siemService,
            IThreatDetectionService threatDetectionService) 
            : base(logger, auditService, authService)
        {
            _siemService = siemService;
            _threatDetectionService = threatDetectionService;
        }

        [HttpGet("siem/status")]
        public async Task<IActionResult> GetSiemStatus()
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var isConnected = await _siemService.TestSiemConnectionAsync();
                var configuration = await _siemService.GetSiemConfigurationAsync();

                return Ok(new
                {
                    isConnected,
                    configuration = new
                    {
                        siemType = configuration.SiemType,
                        isEnabled = configuration.IsEnabled,
                        endpoint = configuration.Endpoint
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetSiemStatus");
            }
        }

        [HttpGet("siem/configuration")]
        public async Task<IActionResult> GetSiemConfiguration()
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var configuration = await _siemService.GetSiemConfigurationAsync();
                return Ok(configuration);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetSiemConfiguration");
            }
        }

        [HttpPost("siem/configuration")]
        public async Task<IActionResult> UpdateSiemConfiguration([FromBody] SiemConfiguration configuration)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var success = await _siemService.UpdateSiemConfigurationAsync(configuration);
                
                await LogAuditEventAsync("SIEM_CONFIG_UPDATE", "SIEM Configuration", 
                    $"Updated SIEM configuration: {configuration.SiemType}");

                if (success)
                {
                    return Ok(new { message = "SIEM configuration updated successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to update SIEM configuration" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "UpdateSiemConfiguration");
            }
        }

        [HttpGet("threats/active")]
        public async Task<IActionResult> GetActiveThreats()
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var threats = await _threatDetectionService.GetActiveThreatsAsync();
                return Ok(threats);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetActiveThreats");
            }
        }

        [HttpGet("threats/detection-rules")]
        public async Task<IActionResult> GetThreatDetectionRules()
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var rules = await _threatDetectionService.GetThreatDetectionRulesAsync();
                return Ok(rules);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetThreatDetectionRules");
            }
        }

        [HttpPost("threats/detection-rules")]
        public async Task<IActionResult> UpdateThreatDetectionRules([FromBody] List<ThreatDetectionRule> rules)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var success = await _threatDetectionService.UpdateThreatDetectionRulesAsync(rules);
                
                await LogAuditEventAsync("THREAT_RULES_UPDATE", "Threat Detection Rules", 
                    $"Updated {rules.Count} threat detection rules");

                if (success)
                {
                    return Ok(new { message = "Threat detection rules updated successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to update threat detection rules" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "UpdateThreatDetectionRules");
            }
        }

        [HttpPost("threats/{threatId}/acknowledge")]
        public async Task<IActionResult> AcknowledgeThreat(int threatId)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var success = await _threatDetectionService.UpdateThreatDetectionStatusAsync(
                    threatId, ThreatStatus.Investigating, GetCurrentUsername() ?? "Unknown");
                
                await LogAuditEventAsync("THREAT_ACKNOWLEDGED", "Threat Detection", 
                    $"Acknowledged threat {threatId}");

                if (success)
                {
                    return Ok(new { message = "Threat acknowledged successfully" });
                }
                else
                {
                    return NotFound(new { error = "Threat not found" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "AcknowledgeThreat");
            }
        }

        [HttpPost("threats/{threatId}/resolve")]
        public async Task<IActionResult> ResolveThreat(int threatId, [FromBody] ThreatResolutionRequest request)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var success = await _threatDetectionService.UpdateThreatDetectionStatusAsync(
                    threatId, ThreatStatus.Resolved, GetCurrentUsername() ?? "Unknown");
                
                await LogAuditEventAsync("THREAT_RESOLVED", "Threat Detection", 
                    $"Resolved threat {threatId}: {request.Resolution}");

                if (success)
                {
                    return Ok(new { message = "Threat resolved successfully" });
                }
                else
                {
                    return NotFound(new { error = "Threat not found" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ResolveThreat");
            }
        }

        [HttpGet("threat-intelligence")]
        public async Task<IActionResult> GetThreatIntelligence()
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var threats = await _siemService.GetThreatIntelligenceAsync();
                return Ok(threats);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetThreatIntelligence");
            }
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetSecurityAlerts()
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var alerts = await _siemService.GetSecurityAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetSecurityAlerts");
            }
        }

        [HttpPost("alerts/{alertId}/acknowledge")]
        public async Task<IActionResult> AcknowledgeSecurityAlert(int alertId)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var success = await _siemService.AcknowledgeSecurityAlertAsync(alertId, GetCurrentUsername() ?? "Unknown");
                
                await LogAuditEventAsync("SECURITY_ALERT_ACKNOWLEDGED", "Security Alert", 
                    $"Acknowledged security alert {alertId}");

                if (success)
                {
                    return Ok(new { message = "Security alert acknowledged successfully" });
                }
                else
                {
                    return NotFound(new { error = "Security alert not found" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "AcknowledgeSecurityAlert");
            }
        }

        [HttpPost("alerts/{alertId}/resolve")]
        public async Task<IActionResult> ResolveSecurityAlert(int alertId, [FromBody] AlertResolutionRequest request)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var success = await _siemService.ResolveSecurityAlertAsync(alertId, GetCurrentUsername() ?? "Unknown", request.Resolution);
                
                await LogAuditEventAsync("SECURITY_ALERT_RESOLVED", "Security Alert", 
                    $"Resolved security alert {alertId}: {request.Resolution}");

                if (success)
                {
                    return Ok(new { message = "Security alert resolved successfully" });
                }
                else
                {
                    return NotFound(new { error = "Security alert not found" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ResolveSecurityAlert");
            }
        }

        [HttpPost("analyze/security-events")]
        public async Task<IActionResult> AnalyzeSecurityEvents([FromBody] List<SecurityEvent> events)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var threats = await _threatDetectionService.AnalyzeSecurityEventsAsync(events);
                
                await LogAuditEventAsync("SECURITY_ANALYSIS", "Security Events", 
                    $"Analyzed {events.Count} security events, found {threats.Count} threats");

                return Ok(threats);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "AnalyzeSecurityEvents");
            }
        }

        [HttpPost("analyze/audit-logs")]
        public async Task<IActionResult> AnalyzeAuditLogs([FromBody] List<AuditLog> logs)
        {
            try
            {
                if (!await HasPermissionAsync("SecurityAudit"))
                {
                    return Forbid();
                }

                var threats = await _threatDetectionService.AnalyzeAuditLogsAsync(logs);
                
                await LogAuditEventAsync("AUDIT_ANALYSIS", "Audit Logs", 
                    $"Analyzed {logs.Count} audit logs, found {threats.Count} threats");

                return Ok(threats);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "AnalyzeAuditLogs");
            }
        }
    }

    public class ThreatResolutionRequest
    {
        public string Resolution { get; set; } = string.Empty;
    }

    public class AlertResolutionRequest
    {
        public string Resolution { get; set; } = string.Empty;
    }
}
