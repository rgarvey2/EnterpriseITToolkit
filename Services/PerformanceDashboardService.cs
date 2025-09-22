using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;

namespace EnterpriseITToolkit.Services
{
    public interface IPerformanceDashboardService
    {
        Task<PerformanceSnapshot> GetCurrentMetricsAsync();
        Task<SystemResourceUsage> GetSystemResourceUsageAsync();
        Task<NetworkPerformance> GetNetworkPerformanceAsync();
        Task<DiskPerformance> GetDiskPerformanceAsync();
        Task<List<ProcessInfo>> GetTopProcessesAsync(int count = 10);
        Task<PerformanceAlert[]> CheckPerformanceAlertsAsync();
    }

    public class PerformanceDashboardService : IPerformanceDashboardService
    {
        private readonly ILogger<PerformanceDashboardService> _logger;
        private readonly ICorrelationService _correlationService;

        public PerformanceDashboardService(ILogger<PerformanceDashboardService> logger, ICorrelationService correlationService)
        {
            _logger = logger;
            _correlationService = correlationService;
        }

        public async Task<PerformanceSnapshot> GetCurrentMetricsAsync()
        {
            using var scope = _correlationService.CreateCorrelationScope("GetCurrentMetrics");
            
            try
            {
                _correlationService.LogWithCorrelation(LogLevel.Information, "Gathering current performance metrics");

                var snapshot = new PerformanceSnapshot
                {
                    Timestamp = DateTime.UtcNow,
                    SystemResources = await GetSystemResourceUsageAsync(),
                    NetworkPerformance = await GetNetworkPerformanceAsync(),
                    DiskPerformance = await GetDiskPerformanceAsync(),
                    TopProcesses = await GetTopProcessesAsync(5),
                    Alerts = await CheckPerformanceAlertsAsync()
                };

                _correlationService.LogWithCorrelation(LogLevel.Information, 
                    "Performance snapshot completed. CPU: {Cpu}%, Memory: {Memory}%, Disk: {Disk}%", 
                    snapshot.SystemResources.CpuUsage, 
                    snapshot.SystemResources.MemoryUsage, 
                    snapshot.DiskPerformance.AverageUsage);

                return snapshot;
            }
            catch (Exception ex)
            {
                _correlationService.LogWithCorrelation(LogLevel.Error, ex, "Error gathering performance metrics");
                throw;
            }
        }

        public async Task<SystemResourceUsage> GetSystemResourceUsageAsync()
        {
            try
            {
                var usage = new SystemResourceUsage();

                // CPU Usage
                using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue(); // First call returns 0
                await Task.Delay(1000);
                usage.CpuUsage = Math.Round(cpuCounter.NextValue(), 2);

                // Memory Usage
                var totalMemory = GC.GetTotalMemory(false);
                var workingSet = Process.GetCurrentProcess().WorkingSet64;
                var systemMemory = GC.GetTotalMemory(false);
                
                usage.MemoryUsage = Math.Round((double)workingSet / systemMemory * 100, 2);
                usage.AvailableMemory = systemMemory - workingSet;
                usage.TotalMemory = systemMemory;

                // Get system memory info from WMI
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var totalVisibleMemory = Convert.ToInt64(obj["TotalVisibleMemorySize"]) * 1024;
                    var freePhysicalMemory = Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024;
                    
                    usage.SystemTotalMemory = totalVisibleMemory;
                    usage.SystemAvailableMemory = freePhysicalMemory;
                    usage.SystemMemoryUsage = Math.Round((double)(totalVisibleMemory - freePhysicalMemory) / totalVisibleMemory * 100, 2);
                    break;
                }

                // Thread Count
                usage.ThreadCount = Process.GetCurrentProcess().Threads.Count;

                // Handle Count
                usage.HandleCount = Process.GetCurrentProcess().HandleCount;

                return usage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system resource usage");
                return new SystemResourceUsage();
            }
        }

        public Task<NetworkPerformance> GetNetworkPerformanceAsync()
        {
            try
            {
                var performance = new NetworkPerformance();

                // Get network interface statistics
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var ni in interfaces)
                {
                    var stats = ni.GetIPStatistics();
                    
                    performance.TotalBytesReceived += stats.BytesReceived;
                    performance.TotalBytesSent += stats.BytesSent;
                    performance.TotalPacketsReceived += stats.IncomingPacketsDiscarded + stats.IncomingPacketsWithErrors;
                    performance.TotalPacketsSent += stats.OutgoingPacketsDiscarded + stats.OutgoingPacketsWithErrors;
                    performance.TotalErrors += stats.IncomingPacketsWithErrors + stats.OutgoingPacketsWithErrors;
                }

                // Calculate rates (this would need to be tracked over time for accurate rates)
                performance.BytesReceivedPerSecond = 0; // Would need previous measurement
                performance.BytesSentPerSecond = 0; // Would need previous measurement

                return Task.FromResult(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network performance");
                return Task.FromResult(new NetworkPerformance());
            }
        }

        public Task<DiskPerformance> GetDiskPerformanceAsync()
        {
            try
            {
                var performance = new DiskPerformance();
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

                foreach (var drive in drives)
                {
                    var driveUsage = new DriveUsage
                    {
                        DriveLetter = drive.Name,
                        TotalSize = drive.TotalSize,
                        FreeSpace = drive.AvailableFreeSpace,
                        UsedSpace = drive.TotalSize - drive.AvailableFreeSpace,
                        UsagePercentage = Math.Round((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100, 2),
                        DriveType = drive.DriveType.ToString(),
                        FileSystem = drive.DriveFormat
                    };

                    performance.DriveUsages.Add(driveUsage);
                }

                performance.AverageUsage = performance.DriveUsages.Any() ? 
                    Math.Round(performance.DriveUsages.Average(d => d.UsagePercentage), 2) : 0;

                return Task.FromResult(performance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disk performance");
                return Task.FromResult(new DiskPerformance());
            }
        }

        public Task<List<ProcessInfo>> GetTopProcessesAsync(int count = 10)
        {
            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => !p.HasExited)
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(count)
                    .Select(p => new ProcessInfo
                    {
                        Name = p.ProcessName,
                        Id = p.Id,
                        MemoryUsage = p.WorkingSet64,
                        CpuUsage = 0, // Would need to calculate over time
                        ThreadCount = p.Threads.Count,
                        HandleCount = p.HandleCount,
                        StartTime = p.StartTime,
                        Responding = p.Responding
                    })
                    .ToList();

                return Task.FromResult(processes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top processes");
                return Task.FromResult(new List<ProcessInfo>());
            }
        }

        public async Task<PerformanceAlert[]> CheckPerformanceAlertsAsync()
        {
            var alerts = new List<PerformanceAlert>();

            try
            {
                var systemResources = await GetSystemResourceUsageAsync();
                var diskPerformance = await GetDiskPerformanceAsync();

                // CPU Alert
                if (systemResources.CpuUsage > 90)
                {
                    alerts.Add(new PerformanceAlert
                    {
                        Type = "CPU",
                        Severity = "High",
                        Message = $"High CPU usage: {systemResources.CpuUsage}%",
                        Timestamp = DateTime.UtcNow,
                        Recommendation = "Check for runaway processes or consider upgrading hardware"
                    });
                }
                else if (systemResources.CpuUsage > 80)
                {
                    alerts.Add(new PerformanceAlert
                    {
                        Type = "CPU",
                        Severity = "Medium",
                        Message = $"Elevated CPU usage: {systemResources.CpuUsage}%",
                        Timestamp = DateTime.UtcNow,
                        Recommendation = "Monitor system performance and check running processes"
                    });
                }

                // Memory Alert
                if (systemResources.SystemMemoryUsage > 90)
                {
                    alerts.Add(new PerformanceAlert
                    {
                        Type = "Memory",
                        Severity = "High",
                        Message = $"High memory usage: {systemResources.SystemMemoryUsage}%",
                        Timestamp = DateTime.UtcNow,
                        Recommendation = "Close unnecessary applications or consider adding more RAM"
                    });
                }

                // Disk Alert
                foreach (var drive in diskPerformance.DriveUsages)
                {
                    if (drive.UsagePercentage > 95)
                    {
                        alerts.Add(new PerformanceAlert
                        {
                            Type = "Disk",
                            Severity = "Critical",
                            Message = $"Critical disk usage on {drive.DriveLetter}: {drive.UsagePercentage}%",
                            Timestamp = DateTime.UtcNow,
                            Recommendation = "Free up disk space immediately to prevent system issues"
                        });
                    }
                    else if (drive.UsagePercentage > 85)
                    {
                        alerts.Add(new PerformanceAlert
                        {
                            Type = "Disk",
                            Severity = "High",
                            Message = $"High disk usage on {drive.DriveLetter}: {drive.UsagePercentage}%",
                            Timestamp = DateTime.UtcNow,
                            Recommendation = "Consider cleaning up disk space or expanding storage"
                        });
                    }
                }

                return alerts.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking performance alerts");
                return alerts.ToArray();
            }
        }
    }

    public class PerformanceSnapshot
    {
        public DateTime Timestamp { get; set; }
        public SystemResourceUsage SystemResources { get; set; } = new();
        public NetworkPerformance NetworkPerformance { get; set; } = new();
        public DiskPerformance DiskPerformance { get; set; } = new();
        public List<ProcessInfo> TopProcesses { get; set; } = new();
        public PerformanceAlert[] Alerts { get; set; } = Array.Empty<PerformanceAlert>();
        
        // Additional properties for API compatibility
        public double CpuUsage => SystemResources.CpuUsage;
        public double MemoryUsage => SystemResources.MemoryUsage;
        public double DiskUsage => DiskPerformance.UsagePercentage;
        public double NetworkUsage => NetworkPerformance.UsagePercentage;
    }

    public class SystemResourceUsage
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public long AvailableMemory { get; set; }
        public long TotalMemory { get; set; }
        public long SystemTotalMemory { get; set; }
        public long SystemAvailableMemory { get; set; }
        public double SystemMemoryUsage { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
    }

    public class NetworkPerformance
    {
        public long TotalBytesReceived { get; set; }
        public long TotalBytesSent { get; set; }
        public long TotalPacketsReceived { get; set; }
        public long TotalPacketsSent { get; set; }
        public long TotalErrors { get; set; }
        public double BytesReceivedPerSecond { get; set; }
        public double BytesSentPerSecond { get; set; }
        public double UsagePercentage { get; set; }
    }

    public class DiskPerformance
    {
        public List<DriveUsage> DriveUsages { get; set; } = new();
        public double AverageUsage { get; set; }
        public double UsagePercentage => AverageUsage;
    }

    public class DriveUsage
    {
        public string DriveLetter { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public long FreeSpace { get; set; }
        public long UsedSpace { get; set; }
        public double UsagePercentage { get; set; }
        public string DriveType { get; set; } = string.Empty;
        public string FileSystem { get; set; } = string.Empty;
    }

    public class ProcessInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public long MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public DateTime StartTime { get; set; }
        public bool Responding { get; set; }
        
        // Additional properties for API compatibility
        public int ProcessId => Id;
    }

    public class PerformanceAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }
}
