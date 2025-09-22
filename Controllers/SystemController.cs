using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : BaseApiController
    {
        private readonly ISystemHealthService _systemHealthService;
        private readonly IPerformanceDashboardService _performanceService;
        private readonly IWorkstationService _workstationService;

        public SystemController(
            ILogger<SystemController> logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService,
            ISystemHealthService systemHealthService,
            IPerformanceDashboardService performanceService,
            IWorkstationService workstationService)
            : base(logger, auditService, authService)
        {
            _systemHealthService = systemHealthService;
            _performanceService = performanceService;
            _workstationService = workstationService;
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var health = await _systemHealthService.GetSystemHealthAsync();
                
                await LogAuditEventAsync("SYSTEM_HEALTH_CHECK", "System", "System health check performed", true);
                
                return Ok(new
                {
                    success = true,
                    health = new
                    {
                        overall = health.OverallHealth,
                        cpu = health.CpuUsage,
                        memory = health.MemoryUsage,
                        disk = health.DiskUsage,
                        network = health.NetworkStatus,
                        services = health.ServiceStatus,
                        timestamp = health.Timestamp
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetSystemHealth");
            }
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceMetrics()
        {
            try
            {
                var metrics = await _performanceService.GetCurrentMetricsAsync();
                
                await LogAuditEventAsync("PERFORMANCE_METRICS", "System", "Performance metrics retrieved", true);
                
                return Ok(new
                {
                    success = true,
                    metrics = new
                    {
                        cpu = metrics.CpuUsage,
                        memory = metrics.MemoryUsage,
                        disk = metrics.DiskUsage,
                        network = metrics.NetworkUsage,
                        timestamp = metrics.Timestamp
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetPerformanceMetrics");
            }
        }

        [HttpGet("processes")]
        public async Task<IActionResult> GetTopProcesses()
        {
            try
            {
                var processes = await _performanceService.GetTopProcessesAsync();
                
                await LogAuditEventAsync("PROCESS_LIST", "System", "Top processes retrieved", true);
                
                return Ok(new
                {
                    success = true,
                    processes = processes.Select(p => new
                    {
                        name = p.Name,
                        pid = p.ProcessId,
                        cpuUsage = p.CpuUsage,
                        memoryUsage = p.MemoryUsage,
                        startTime = p.StartTime
                    })
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetTopProcesses");
            }
        }

        [HttpGet("workstation-info")]
        public async Task<IActionResult> GetWorkstationInfo()
        {
            try
            {
                var info = await _workstationService.GetWorkstationInfoAsync();
                
                await LogAuditEventAsync("WORKSTATION_INFO", "System", "Workstation information retrieved", true);
                
                return Ok(new
                {
                    success = true,
                    workstation = new
                    {
                        computerName = info.ComputerName,
                        operatingSystem = info.OperatingSystem,
                        processor = info.Processor,
                        memory = info.Memory,
                        diskSpace = info.DiskSpace,
                        networkAdapters = info.NetworkAdapters,
                        lastUpdated = info.LastUpdated
                    }
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetWorkstationInfo");
            }
        }

        [HttpGet("software-inventory")]
        public async Task<IActionResult> GetSoftwareInventory()
        {
            try
            {
                var software = await _workstationService.GetInstalledSoftwareAsync();
                
                await LogAuditEventAsync("SOFTWARE_INVENTORY", "System", "Software inventory retrieved", true);
                
                return Ok(new
                {
                    success = true,
                    software = software.Software.Select(s => new
                    {
                        name = s.Name,
                        version = s.Version,
                        publisher = s.Publisher,
                        installedDate = s.InstalledDate,
                        size = s.Size
                    })
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetSoftwareInventory");
            }
        }

        [HttpPost("optimize-performance")]
        public async Task<IActionResult> OptimizePerformance()
        {
            try
            {
                var result = await _workstationService.OptimizePerformanceAsync();
                
                await LogAuditEventAsync("PERFORMANCE_OPTIMIZATION", "System", "Performance optimization performed", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    optimizations = result.Optimizations
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "OptimizePerformance");
            }
        }

        [HttpPost("backup-registry")]
        public async Task<IActionResult> BackupRegistry()
        {
            try
            {
                var result = await _workstationService.BackupRegistryAsync("C:\\Backups\\Registry");
                
                await LogAuditEventAsync("REGISTRY_BACKUP", "System", "Registry backup performed", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    message = result.Success ? "Registry backup completed successfully" : result.Error,
                    backupPath = result.BackupPath
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "BackupRegistry");
            }
        }

        [HttpPost("create-restore-point")]
        public async Task<IActionResult> CreateSystemRestorePoint([FromBody] CreateRestorePointRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Description))
                {
                    return BadRequest(new { error = "Description is required" });
                }

                var result = await _workstationService.CreateSystemRestorePointAsync(request.Description);
                
                await LogAuditEventAsync("SYSTEM_RESTORE_POINT", "System", $"System restore point created: {request.Description}", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    restorePointId = result.RestorePointId
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateSystemRestorePoint");
            }
        }
    }

    public class CreateRestorePointRequest
    {
        public string Description { get; set; } = string.Empty;
    }
}
