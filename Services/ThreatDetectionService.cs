using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public class ThreatDetectionService : IThreatDetectionService
    {
        private readonly ILogger<ThreatDetectionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISiemService _siemService;
        private readonly List<ThreatDetectionRule> _detectionRules;
        private readonly List<ThreatDetection> _activeThreats;

        public ThreatDetectionService(
            ILogger<ThreatDetectionService> logger, 
            IConfiguration configuration,
            ISiemService siemService)
        {
            _logger = logger;
            _configuration = configuration;
            _siemService = siemService;
            _detectionRules = LoadDefaultRules();
            _activeThreats = new List<ThreatDetection>();
        }

        public async Task<List<ThreatDetection>> AnalyzeSecurityEventsAsync(List<SecurityEvent> events)
        {
            var threats = new List<ThreatDetection>();

            try
            {
                _logger.LogInformation("Analyzing {Count} security events for threats", events.Count);

                // Group events by type for analysis
                var loginEvents = events.Where(e => e.EventType.Contains("LOGIN")).ToList();
                var accessEvents = events.Where(e => e.EventType.Contains("ACCESS")).ToList();
                var systemEvents = events.Where(e => e.EventType.Contains("SYSTEM")).ToList();

                // Analyze for brute force attacks
                var bruteForceThreats = await DetectBruteForceFromEvents(loginEvents);
                threats.AddRange(bruteForceThreats);

                // Analyze for privilege escalation
                var privilegeThreats = await DetectPrivilegeEscalationFromEvents(accessEvents);
                threats.AddRange(privilegeThreats);

                // Analyze for malware activity
                var malwareThreats = await DetectMalwareFromEvents(systemEvents);
                threats.AddRange(malwareThreats);

                _logger.LogInformation("Detected {Count} threats from security events", threats.Count);
                return threats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing security events for threats");
                return threats;
            }
        }

        public async Task<List<ThreatDetection>> AnalyzeAuditLogsAsync(List<AuditLog> logs)
        {
            var threats = new List<ThreatDetection>();

            try
            {
                _logger.LogInformation("Analyzing {Count} audit logs for threats", logs.Count);

                // Group logs by action for analysis
                var fileAccessLogs = logs.Where(l => l.Action.Contains("FILE_ACCESS")).ToList();
                var userManagementLogs = logs.Where(l => l.Action.Contains("USER_")).ToList();
                var systemLogs = logs.Where(l => l.Action.Contains("SYSTEM_")).ToList();

                // Analyze for data exfiltration
                var dataExfiltrationThreats = await DetectDataExfiltrationFromLogs(fileAccessLogs);
                threats.AddRange(dataExfiltrationThreats);

                // Analyze for privilege escalation
                var privilegeThreats = await DetectPrivilegeEscalationFromLogs(userManagementLogs);
                threats.AddRange(privilegeThreats);

                _logger.LogInformation("Detected {Count} threats from audit logs", threats.Count);
                return threats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing audit logs for threats");
                return threats;
            }
        }

        public async Task<ThreatDetection> DetectAnomalousLoginAsync(string username, string ipAddress, DateTime timestamp)
        {
            try
            {
                _logger.LogInformation("Analyzing login for user {Username} from IP {IpAddress}", username, ipAddress);

                var threat = new ThreatDetection
                {
                    ThreatType = "AnomalousLogin",
                    Title = "Anomalous Login Detected",
                    Description = $"Unusual login pattern detected for user {username} from IP {ipAddress}",
                    Severity = "Medium",
                    Status = ThreatStatus.New,
                    Confidence = "Medium",
                    DetectedAt = timestamp,
                    AffectedSystems = new List<string> { Environment.MachineName },
                    Indicators = new List<string> { ipAddress, username },
                    Evidence = new Dictionary<string, object>
                    {
                        ["username"] = username,
                        ["ip_address"] = ipAddress,
                        ["timestamp"] = timestamp,
                        ["analysis_reason"] = "Geographic anomaly or unusual time"
                    },
                    RecommendedActions = new List<string>
                    {
                        "Verify user identity",
                        "Check for account compromise",
                        "Review recent login history",
                        "Consider additional authentication"
                    },
                    Source = "ThreatDetectionService"
                };

                _activeThreats.Add(threat);
                await _siemService.SendSecurityEventAsync(new SecurityEvent
                {
                    EventType = "THREAT_DETECTED",
                    Severity = "Medium",
                    Description = $"Anomalous login detected: {threat.Description}",
                    Timestamp = timestamp
                });

                return threat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting anomalous login for user {Username}", username);
                throw;
            }
        }

        public Task<ThreatDetection?> DetectBruteForceAttackAsync(string target, List<SecurityEvent> events)
        {
            try
            {
                var failedAttempts = events.Where(e => 
                    e.EventType.Contains("LOGIN") && 
                    e.Severity == "FAILED" &&
                    e.Description != null && e.Description.Contains(target)).ToList();

                if (failedAttempts.Count >= 5) // Threshold for brute force
                {
                    var threat = new ThreatDetection
                    {
                        ThreatType = "BruteForce",
                        Title = "Brute Force Attack Detected",
                        Description = $"Multiple failed login attempts detected for target: {target}",
                        Severity = "High",
                        Status = ThreatStatus.New,
                        Confidence = "High",
                        DetectedAt = DateTime.UtcNow,
                        AffectedSystems = new List<string> { target },
                        Indicators = new List<string> { target },
                        Evidence = new Dictionary<string, object>
                        {
                            ["target"] = target,
                            ["failed_attempts"] = failedAttempts.Count,
                            ["time_window"] = "5 minutes",
                            ["attack_pattern"] = "Brute Force"
                        },
                        RecommendedActions = new List<string>
                        {
                            "Block source IP addresses",
                            "Enable account lockout",
                            "Review authentication logs",
                            "Consider rate limiting"
                        },
                        Source = "ThreatDetectionService"
                    };

                    _activeThreats.Add(threat);
                    return Task.FromResult<ThreatDetection?>(threat);
                }

                return Task.FromResult<ThreatDetection?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting brute force attack for target {Target}", target);
                throw;
            }
        }

        public Task<ThreatDetection?> DetectPrivilegeEscalationAsync(List<AuditLog> logs)
        {
            try
            {
                var privilegeLogs = logs.Where(l => 
                    l.Action.Contains("ROLE_ASSIGN") || 
                    l.Action.Contains("PERMISSION_GRANT") ||
                    l.Action.Contains("ADMIN_ACCESS")).ToList();

                if (privilegeLogs.Count > 0)
                {
                    var threat = new ThreatDetection
                    {
                        ThreatType = "PrivilegeEscalation",
                        Title = "Potential Privilege Escalation",
                        Description = "Unusual privilege changes detected in audit logs",
                        Severity = "High",
                        Status = ThreatStatus.New,
                        Confidence = "Medium",
                        DetectedAt = DateTime.UtcNow,
                        AffectedSystems = new List<string> { Environment.MachineName },
                        Indicators = new List<string>(),
                        Evidence = new Dictionary<string, object>
                        {
                            ["privilege_changes"] = privilegeLogs.Count,
                            ["affected_users"] = privilegeLogs.Select(l => l.UserId).Distinct().ToList(),
                            ["time_window"] = "1 hour"
                        },
                        RecommendedActions = new List<string>
                        {
                            "Review user permissions",
                            "Verify authorization requests",
                            "Check for insider threats",
                            "Audit role assignments"
                        },
                        Source = "ThreatDetectionService"
                    };

                    _activeThreats.Add(threat);
                    return Task.FromResult<ThreatDetection?>(threat);
                }

                return Task.FromResult<ThreatDetection?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting privilege escalation");
                throw;
            }
        }

        public Task<ThreatDetection?> DetectDataExfiltrationAsync(List<AuditLog> logs)
        {
            try
            {
                var fileAccessLogs = logs.Where(l => 
                    l.Action.Contains("FILE_ACCESS") || 
                    l.Action.Contains("FILE_DOWNLOAD") ||
                    l.Action.Contains("DATA_EXPORT")).ToList();

                // Look for unusual patterns (large volumes, off-hours, etc.)
                var suspiciousAccess = fileAccessLogs.Where(l => 
                    l.Timestamp.Hour < 6 || l.Timestamp.Hour > 22).ToList();

                if (suspiciousAccess.Count >= 10)
                {
                    var threat = new ThreatDetection
                    {
                        ThreatType = "DataExfiltration",
                        Title = "Potential Data Exfiltration",
                        Description = "Unusual file access patterns detected",
                        Severity = "Critical",
                        Status = ThreatStatus.New,
                        Confidence = "Medium",
                        DetectedAt = DateTime.UtcNow,
                        AffectedSystems = new List<string> { Environment.MachineName },
                        Indicators = new List<string>(),
                        Evidence = new Dictionary<string, object>
                        {
                            ["suspicious_access_count"] = suspiciousAccess.Count,
                            ["off_hours_access"] = true,
                            ["affected_users"] = suspiciousAccess.Select(l => l.UserId).Distinct().ToList()
                        },
                        RecommendedActions = new List<string>
                        {
                            "Review file access logs",
                            "Check for unauthorized data transfers",
                            "Monitor network traffic",
                            "Verify user activities"
                        },
                        Source = "ThreatDetectionService"
                    };

                    _activeThreats.Add(threat);
                    return Task.FromResult<ThreatDetection?>(threat);
                }

                return Task.FromResult<ThreatDetection?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting data exfiltration");
                throw;
            }
        }

        public Task<ThreatDetection?> DetectMalwareActivityAsync(List<SecurityEvent> events)
        {
            try
            {
                var malwareEvents = events.Where(e => 
                    e.EventType.Contains("MALWARE") || 
                    e.EventType.Contains("VIRUS") ||
                    e.EventType.Contains("THREAT")).ToList();

                if (malwareEvents.Any())
                {
                    var threat = new ThreatDetection
                    {
                        ThreatType = "Malware",
                        Title = "Malware Activity Detected",
                        Description = "Malware or threat activity detected in security events",
                        Severity = "Critical",
                        Status = ThreatStatus.New,
                        Confidence = "High",
                        DetectedAt = DateTime.UtcNow,
                        AffectedSystems = new List<string> { Environment.MachineName },
                        Indicators = new List<string>(),
                        Evidence = new Dictionary<string, object>
                        {
                            ["malware_events"] = malwareEvents.Count,
                            ["threat_types"] = malwareEvents.Select(e => e.EventType).Distinct().ToList(),
                            ["affected_systems"] = malwareEvents.Select(e => e.Description ?? "").ToList()
                        },
                        RecommendedActions = new List<string>
                        {
                            "Isolate affected systems",
                            "Run full antivirus scan",
                            "Review system integrity",
                            "Update security signatures"
                        },
                        Source = "ThreatDetectionService"
                    };

                    _activeThreats.Add(threat);
                    return Task.FromResult<ThreatDetection?>(threat);
                }

                return Task.FromResult<ThreatDetection?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting malware activity");
                throw;
            }
        }

        public Task<List<ThreatDetection>> GetActiveThreatsAsync()
        {
            return Task.FromResult(_activeThreats.Where(t => t.Status != ThreatStatus.Resolved).ToList());
        }

        public Task<bool> UpdateThreatDetectionRulesAsync(List<ThreatDetectionRule> rules)
        {
            try
            {
                _detectionRules.Clear();
                _detectionRules.AddRange(rules);
                _logger.LogInformation("Updated {Count} threat detection rules", rules.Count);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating threat detection rules");
                return Task.FromResult(false);
            }
        }

        public Task<List<ThreatDetectionRule>> GetThreatDetectionRulesAsync()
        {
            return Task.FromResult(_detectionRules.ToList());
        }

        public Task<ThreatDetection> CreateThreatDetectionAsync(ThreatDetection threat)
        {
            try
            {
                threat.Id = _activeThreats.Count + 1;
                threat.DetectedAt = DateTime.UtcNow;
                _activeThreats.Add(threat);
                
                _logger.LogInformation("Created new threat detection: {ThreatType}", threat.ThreatType);
                return Task.FromResult(threat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating threat detection");
                throw;
            }
        }

        public Task<bool> UpdateThreatDetectionStatusAsync(int threatId, ThreatStatus status, string updatedBy)
        {
            try
            {
                var threat = _activeThreats.FirstOrDefault(t => t.Id == threatId);
                if (threat != null)
                {
                    threat.Status = status;
                    if (status == ThreatStatus.Resolved)
                    {
                        threat.ResolvedAt = DateTime.UtcNow;
                        threat.ResolvedBy = updatedBy;
                    }
                    
                    _logger.LogInformation("Updated threat {ThreatId} status to {Status} by {User}", threatId, status, updatedBy);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating threat detection status");
                return Task.FromResult(false);
            }
        }

        private List<ThreatDetectionRule> LoadDefaultRules()
        {
            return new List<ThreatDetectionRule>
            {
                new ThreatDetectionRule
                {
                    Id = 1,
                    Name = "Brute Force Detection",
                    Description = "Detects multiple failed login attempts",
                    ThreatType = "BruteForce",
                    Severity = "High",
                    IsEnabled = true,
                    Condition = "failed_login_attempts >= 5",
                    Threshold = 5,
                    TimeWindow = TimeSpan.FromMinutes(5),
                    Actions = new List<string> { "Alert", "Block" },
                    CreatedAt = DateTime.UtcNow
                },
                new ThreatDetectionRule
                {
                    Id = 2,
                    Name = "Privilege Escalation Detection",
                    Description = "Detects unusual privilege changes",
                    ThreatType = "PrivilegeEscalation",
                    Severity = "High",
                    IsEnabled = true,
                    Condition = "privilege_changes >= 3",
                    Threshold = 3,
                    TimeWindow = TimeSpan.FromHours(1),
                    Actions = new List<string> { "Alert", "Log" },
                    CreatedAt = DateTime.UtcNow
                },
                new ThreatDetectionRule
                {
                    Id = 3,
                    Name = "Data Exfiltration Detection",
                    Description = "Detects unusual file access patterns",
                    ThreatType = "DataExfiltration",
                    Severity = "Critical",
                    IsEnabled = true,
                    Condition = "off_hours_access >= 10",
                    Threshold = 10,
                    TimeWindow = TimeSpan.FromHours(24),
                    Actions = new List<string> { "Alert", "Quarantine" },
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        private Task<List<ThreatDetection>> DetectBruteForceFromEvents(List<SecurityEvent> events)
        {
            var threats = new List<ThreatDetection>();
            
            // Group by target and analyze
            var groupedEvents = events.GroupBy(e => ExtractTargetFromEvent(e));
            
            foreach (var group in groupedEvents)
            {
                if (group.Count() >= 5)
                {
                    var threat = DetectBruteForceAttackAsync(group.Key, group.ToList()).Result;
                    if (threat != null)
                    {
                        threats.Add(threat);
                    }
                }
            }
            
            return Task.FromResult(threats);
        }

        private Task<List<ThreatDetection>> DetectPrivilegeEscalationFromEvents(List<SecurityEvent> events)
        {
            var threats = new List<ThreatDetection>();
            
            // Look for privilege-related events
            var privilegeEvents = events.Where(e => 
                e.EventType.Contains("ROLE") || 
                e.EventType.Contains("PERMISSION") ||
                e.EventType.Contains("ADMIN")).ToList();
            
            if (privilegeEvents.Count >= 3)
            {
                var threat = new ThreatDetection
                {
                    ThreatType = "PrivilegeEscalation",
                    Title = "Privilege Escalation Detected",
                    Description = "Multiple privilege changes detected in security events",
                    Severity = "High",
                    Status = ThreatStatus.New,
                    Confidence = "Medium",
                    DetectedAt = DateTime.UtcNow,
                    AffectedSystems = new List<string> { Environment.MachineName },
                    Indicators = new List<string>(),
                    Evidence = new Dictionary<string, object>
                    {
                        ["privilege_events"] = privilegeEvents.Count,
                        ["event_types"] = privilegeEvents.Select(e => e.EventType).Distinct().ToList()
                    },
                    RecommendedActions = new List<string>
                    {
                        "Review user permissions",
                        "Audit role assignments",
                        "Check for unauthorized changes"
                    },
                    Source = "ThreatDetectionService"
                };
                
                threats.Add(threat);
            }
            
            return Task.FromResult(threats);
        }

        private Task<List<ThreatDetection>> DetectMalwareFromEvents(List<SecurityEvent> events)
        {
            var threats = new List<ThreatDetection>();
            
            var malwareEvents = events.Where(e => 
                e.EventType.Contains("MALWARE") || 
                e.EventType.Contains("VIRUS") ||
                e.EventType.Contains("THREAT")).ToList();
            
            if (malwareEvents.Any())
            {
                var threat = DetectMalwareActivityAsync(malwareEvents).Result;
                if (threat != null)
                {
                    threats.Add(threat);
                }
            }
            
            return Task.FromResult(threats);
        }

        private Task<List<ThreatDetection>> DetectPrivilegeEscalationFromLogs(List<AuditLog> logs)
        {
            var threats = new List<ThreatDetection>();
            
            if (logs.Count >= 3)
            {
                var threat = DetectPrivilegeEscalationAsync(logs).Result;
                if (threat != null)
                {
                    threats.Add(threat);
                }
            }
            
            return Task.FromResult(threats);
        }

        private Task<List<ThreatDetection>> DetectDataExfiltrationFromLogs(List<AuditLog> logs)
        {
            var threats = new List<ThreatDetection>();
            
            if (logs.Count >= 10)
            {
                var threat = DetectDataExfiltrationAsync(logs).Result;
                if (threat != null)
                {
                    threats.Add(threat);
                }
            }
            
            return Task.FromResult(threats);
        }

        private string ExtractTargetFromEvent(SecurityEvent securityEvent)
        {
            // Extract target from event description
            if (securityEvent.Description != null && securityEvent.Description.Contains("user:"))
            {
                var parts = securityEvent.Description.Split("user:");
                if (parts.Length > 1)
                {
                    return parts[1].Split(' ')[0];
                }
            }
            
            return securityEvent.UserId ?? "unknown";
        }
    }
}
