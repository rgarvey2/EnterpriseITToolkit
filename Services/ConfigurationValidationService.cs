using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EnterpriseITToolkit.Services
{
    public interface IConfigurationValidationService
    {
        Task<ValidationResult> ValidateConfigurationAsync();
        Task<ValidationResult> ValidateSecuritySettingsAsync();
        Task<ValidationResult> ValidateNetworkSettingsAsync();
        Task<ValidationResult> ValidateLoggingSettingsAsync();
    }

    public class ConfigurationValidationService : IConfigurationValidationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationValidationService> _logger;

        public ConfigurationValidationService(IConfiguration configuration, ILogger<ConfigurationValidationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateConfigurationAsync()
        {
            var result = new ValidationResult { Operation = "Configuration Validation" };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting comprehensive configuration validation");

                // Validate all configuration sections
                var securityResult = await ValidateSecuritySettingsAsync();
                var networkResult = await ValidateNetworkSettingsAsync();
                var loggingResult = await ValidateLoggingSettingsAsync();

                result.ValidationChecks.AddRange(securityResult.ValidationChecks);
                result.ValidationChecks.AddRange(networkResult.ValidationChecks);
                result.ValidationChecks.AddRange(loggingResult.ValidationChecks);

                result.Success = result.ValidationChecks.All(vc => vc.Passed);
                result.Summary = $"Configuration validation completed. {result.ValidationChecks.Count(vc => vc.Passed)}/{result.ValidationChecks.Count} checks passed.";

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;

                _logger.LogInformation("Configuration validation completed in {Time}ms. Success: {Success}", 
                    stopwatch.ElapsedMilliseconds, result.Success);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Error = ex.Message;
                result.ExecutionTime = stopwatch.Elapsed;
                _logger.LogError(ex, "Error during configuration validation");
                return result;
            }
        }

        public Task<ValidationResult> ValidateSecuritySettingsAsync()
        {
            var result = new ValidationResult { Operation = "Security Settings Validation" };

            try
            {
                _logger.LogInformation("Validating security settings");

                // Check required security settings
                var requiredSecuritySettings = new Dictionary<string, string>
                {
                    { "SecuritySettings:AuditLogPath", "Audit log path is required" },
                    { "SecuritySettings:MaxLoginAttempts", "Maximum login attempts must be configured" },
                    { "SecuritySettings:SessionTimeoutMinutes", "Session timeout must be configured" }
                };

                foreach (var setting in requiredSecuritySettings)
                {
                    var value = _configuration[setting.Key];
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        result.ValidationChecks.Add(new ValidationCheck
                        {
                            Name = setting.Key,
                            Passed = false,
                            Message = setting.Value,
                            Severity = "High"
                        });
                    }
                    else
                    {
                        result.ValidationChecks.Add(new ValidationCheck
                        {
                            Name = setting.Key,
                            Passed = true,
                            Message = "Configuration is valid",
                            Severity = "Low"
                        });
                    }
                }

                // Validate audit log path
                var auditLogPath = _configuration["SecuritySettings:AuditLogPath"];
                if (!string.IsNullOrWhiteSpace(auditLogPath))
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(auditLogPath);
                        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                            _logger.LogInformation("Created audit log directory: {Directory}", directory);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ValidationChecks.Add(new ValidationCheck
                        {
                            Name = "SecuritySettings:AuditLogPath",
                            Passed = false,
                            Message = $"Cannot create audit log directory: {ex.Message}",
                            Severity = "High"
                        });
                    }
                }

                // Validate numeric settings
                var maxLoginAttempts = _configuration.GetValue<int>("SecuritySettings:MaxLoginAttempts");
                if (maxLoginAttempts <= 0 || maxLoginAttempts > 10)
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "SecuritySettings:MaxLoginAttempts",
                        Passed = false,
                        Message = "Maximum login attempts must be between 1 and 10",
                        Severity = "Medium"
                    });
                }

                var sessionTimeout = _configuration.GetValue<int>("SecuritySettings:SessionTimeoutMinutes");
                if (sessionTimeout <= 0 || sessionTimeout > 480) // Max 8 hours
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "SecuritySettings:SessionTimeoutMinutes",
                        Passed = false,
                        Message = "Session timeout must be between 1 and 480 minutes",
                        Severity = "Medium"
                    });
                }

                result.Success = result.ValidationChecks.All(vc => vc.Passed);
                result.Summary = $"Security settings validation completed. {result.ValidationChecks.Count(vc => vc.Passed)}/{result.ValidationChecks.Count} checks passed.";

                _logger.LogInformation("Security settings validation completed. Success: {Success}", result.Success);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error validating security settings");
                return Task.FromResult(result);
            }
        }

        public Task<ValidationResult> ValidateNetworkSettingsAsync()
        {
            var result = new ValidationResult { Operation = "Network Settings Validation" };

            try
            {
                _logger.LogInformation("Validating network settings");

                // Validate timeout settings
                var defaultTimeout = _configuration.GetValue<int>("NetworkSettings:DefaultTimeout");
                if (defaultTimeout <= 0 || defaultTimeout > 30000) // Max 30 seconds
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "NetworkSettings:DefaultTimeout",
                        Passed = false,
                        Message = "Default timeout must be between 1 and 30000 milliseconds",
                        Severity = "Medium"
                    });
                }
                else
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "NetworkSettings:DefaultTimeout",
                        Passed = true,
                        Message = "Timeout setting is valid",
                        Severity = "Low"
                    });
                }

                // Validate max concurrent pings
                var maxConcurrentPings = _configuration.GetValue<int>("NetworkSettings:MaxConcurrentPings");
                if (maxConcurrentPings <= 0 || maxConcurrentPings > 50)
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "NetworkSettings:MaxConcurrentPings",
                        Passed = false,
                        Message = "Max concurrent pings must be between 1 and 50",
                        Severity = "Medium"
                    });
                }
                else
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "NetworkSettings:MaxConcurrentPings",
                        Passed = true,
                        Message = "Max concurrent pings setting is valid",
                        Severity = "Low"
                    });
                }

                // Validate allowed hosts
                var allowedHosts = _configuration.GetSection("NetworkSettings:AllowedHosts").Get<string[]>();
                if (allowedHosts == null || allowedHosts.Length == 0)
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "NetworkSettings:AllowedHosts",
                        Passed = false,
                        Message = "At least one allowed host must be configured",
                        Severity = "High"
                    });
                }
                else
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "NetworkSettings:AllowedHosts",
                        Passed = true,
                        Message = $"{allowedHosts.Length} allowed hosts configured",
                        Severity = "Low"
                    });
                }

                result.Success = result.ValidationChecks.All(vc => vc.Passed);
                result.Summary = $"Network settings validation completed. {result.ValidationChecks.Count(vc => vc.Passed)}/{result.ValidationChecks.Count} checks passed.";

                _logger.LogInformation("Network settings validation completed. Success: {Success}", result.Success);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error validating network settings");
                return Task.FromResult(result);
            }
        }

        public Task<ValidationResult> ValidateLoggingSettingsAsync()
        {
            var result = new ValidationResult { Operation = "Logging Settings Validation" };

            try
            {
                _logger.LogInformation("Validating logging settings");

                // Validate log level
                var logLevel = _configuration["Logging:LogLevel:Default"];
                var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
                
                if (string.IsNullOrWhiteSpace(logLevel) || !validLogLevels.Contains(logLevel))
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "Logging:LogLevel:Default",
                        Passed = false,
                        Message = $"Log level must be one of: {string.Join(", ", validLogLevels)}",
                        Severity = "Medium"
                    });
                }
                else
                {
                    result.ValidationChecks.Add(new ValidationCheck
                    {
                        Name = "Logging:LogLevel:Default",
                        Passed = true,
                        Message = $"Log level '{logLevel}' is valid",
                        Severity = "Low"
                    });
                }

                // Validate Serilog configuration
                var serilogPath = _configuration["Serilog:WriteTo:1:Args:path"];
                if (!string.IsNullOrWhiteSpace(serilogPath))
                {
                    try
                    {
                        var logDirectory = Path.GetDirectoryName(serilogPath);
                        if (!string.IsNullOrWhiteSpace(logDirectory) && !Directory.Exists(logDirectory))
                        {
                            Directory.CreateDirectory(logDirectory);
                            _logger.LogInformation("Created log directory: {Directory}", logDirectory);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ValidationChecks.Add(new ValidationCheck
                        {
                            Name = "Serilog:WriteTo:1:Args:path",
                            Passed = false,
                            Message = $"Cannot create log directory: {ex.Message}",
                            Severity = "High"
                        });
                    }
                }

                result.Success = result.ValidationChecks.All(vc => vc.Passed);
                result.Summary = $"Logging settings validation completed. {result.ValidationChecks.Count(vc => vc.Passed)}/{result.ValidationChecks.Count} checks passed.";

                _logger.LogInformation("Logging settings validation completed. Success: {Success}", result.Success);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error validating logging settings");
                return Task.FromResult(result);
            }
        }
    }

    public class ValidationResult
    {
        public string Operation { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public List<ValidationCheck> ValidationChecks { get; set; } = new();
    }

    public class ValidationCheck
    {
        public string Name { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }
}
