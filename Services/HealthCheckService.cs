using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EnterpriseITToolkit.Data;
using System.Net.NetworkInformation;

namespace EnterpriseITToolkit.Services
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly EnterpriseDbContext _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(EnterpriseDbContext context, ILogger<DatabaseHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Database.CanConnectAsync(cancellationToken);
                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database connection failed", ex);
            }
        }
    }

    public class NetworkHealthCheck : IHealthCheck
    {
        private readonly ILogger<NetworkHealthCheck> _logger;

        public NetworkHealthCheck(ILogger<NetworkHealthCheck> logger)
        {
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 5000);
                
                if (reply.Status == IPStatus.Success)
                {
                    return HealthCheckResult.Healthy($"Network connectivity is healthy (RTT: {reply.RoundtripTime}ms)");
                }
                else
                {
                    return HealthCheckResult.Degraded($"Network connectivity issues (Status: {reply.Status})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network health check failed");
                return HealthCheckResult.Unhealthy("Network connectivity failed", ex);
            }
        }
    }

    public class DiskSpaceHealthCheck : IHealthCheck
    {
        private readonly ILogger<DiskSpaceHealthCheck> _logger;

        public DiskSpaceHealthCheck(ILogger<DiskSpaceHealthCheck> logger)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                var unhealthyDrives = new List<string>();

                foreach (var drive in drives.Where(d => d.IsReady))
                {
                    var freeSpacePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                    
                    if (freeSpacePercent < 10)
                    {
                        unhealthyDrives.Add($"{drive.Name} ({freeSpacePercent:F1}% free)");
                    }
                }

                if (unhealthyDrives.Any())
                {
                    return Task.FromResult(HealthCheckResult.Degraded($"Low disk space: {string.Join(", ", unhealthyDrives)}"));
                }

                return Task.FromResult(HealthCheckResult.Healthy("Disk space is healthy"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Disk space health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy("Disk space check failed", ex));
            }
        }
    }

    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly ILogger<MemoryHealthCheck> _logger;

        public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var totalMemory = GC.GetTotalMemory(false);
                
                // Check if memory usage is reasonable (less than 1GB)
                if (workingSet > 1024 * 1024 * 1024)
                {
                    return Task.FromResult(HealthCheckResult.Degraded($"High memory usage: {workingSet / 1024 / 1024}MB"));
                }

                return Task.FromResult(HealthCheckResult.Healthy($"Memory usage is healthy: {workingSet / 1024 / 1024}MB"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Memory health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy("Memory check failed", ex));
            }
        }
    }

    public class ServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<ServiceHealthCheck> _logger;

        public ServiceHealthCheck(ILogger<ServiceHealthCheck> logger)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var criticalServices = new[]
                {
                    "Spooler",      // Print Spooler
                    "RpcSs",        // Remote Procedure Call
                    "LanmanServer", // Server
                    "LanmanWorkstation" // Workstation
                };

                var unhealthyServices = new List<string>();

                foreach (var serviceName in criticalServices)
                {
                    try
                    {
                        using var service = new System.ServiceProcess.ServiceController(serviceName);
                        if (service.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                        {
                            unhealthyServices.Add($"{serviceName} ({service.Status})");
                        }
                    }
                    catch
                    {
                        // Service not found or not accessible
                        unhealthyServices.Add($"{serviceName} (Not Found)");
                    }
                }

                if (unhealthyServices.Any())
                {
                    return Task.FromResult(HealthCheckResult.Degraded($"Critical services issues: {string.Join(", ", unhealthyServices)}"));
                }

                return Task.FromResult(HealthCheckResult.Healthy("Critical services are running"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy("Service check failed", ex));
            }
        }
    }
}
