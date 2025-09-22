namespace EnterpriseITToolkit.Services
{
    public interface ISystemHealthService
    {
        Task<SystemHealthResult> GetSystemHealthAsync();
        Task<DiskCleanupResult> RunDiskCleanupAsync();
        Task<PerformanceMetrics> GetPerformanceMetricsAsync();
    }

    public class SystemHealthResult
    {
        public bool IsHealthy { get; set; }
        public bool Success { get; set; }
        public string ComputerName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<HealthCheck> HealthChecks { get; set; } = new();
        public string Error { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public string OverallHealth => IsHealthy ? "Healthy" : "Unhealthy";
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public string NetworkStatus { get; set; } = "Unknown";
        public string ServiceStatus { get; set; } = "Unknown";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class HealthCheck
    {
        public string Name { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class DiskCleanupResult
    {
        public bool Success { get; set; }
        public long SpaceFreedBytes { get; set; }
        public List<string> CleanedItems { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    public class PerformanceMetrics
    {
        public double CpuUsage { get; set; }
        public long MemoryUsageBytes { get; set; }
        public long AvailableMemoryBytes { get; set; }
        public long DiskUsageBytes { get; set; }
        public long AvailableDiskBytes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
