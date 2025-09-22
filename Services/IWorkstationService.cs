namespace EnterpriseITToolkit.Services
{
    public interface IWorkstationService
    {
        Task<WorkstationInfo> GetWorkstationInfoAsync();
        Task<SoftwareInventory> GetInstalledSoftwareAsync();
        Task<UpdateStatus> CheckWindowsUpdatesAsync();
        Task<UpdateResult> InstallUpdatesAsync();
        Task<ServiceStatus> ManageServiceAsync(string serviceName, ServiceAction action);
        Task<RegistryBackup> BackupRegistryAsync(string backupPath);
        Task<SystemRestore> CreateSystemRestorePointAsync(string description);
        Task<PerformanceOptimization> OptimizePerformanceAsync();
        Task<DiskDefragmentation> DefragmentDiskAsync(string driveLetter);
        Task<StartupOptimization> OptimizeStartupAsync();
    }

    public class WorkstationInfo
    {
        public string ComputerName { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string BIOSVersion { get; set; } = string.Empty;
        public DateTime LastBootTime { get; set; }
        public TimeSpan Uptime { get; set; }
        public List<NetworkAdapter> NetworkAdapters { get; set; } = new();
        public List<DiskDrive> DiskDrives { get; set; } = new();
        public List<MemoryModule> MemoryModules { get; set; } = new();
        
        // Additional properties for API compatibility
        public string OperatingSystem => OSVersion;
        public string Processor => $"{Manufacturer} {Model}";
        public string Memory => $"{MemoryModules.Sum(m => m.Capacity) / (1024 * 1024 * 1024)} GB";
        public string DiskSpace => $"{DiskDrives.Sum(d => d.TotalSize) / (1024 * 1024 * 1024)} GB";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class NetworkAdapter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MACAddress { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long Speed { get; set; }
    }

    public class DiskDrive
    {
        public string Letter { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string FileSystem { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public long FreeSpace { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class MemoryModule
    {
        public string Location { get; set; } = string.Empty;
        public long Capacity { get; set; }
        public string Speed { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
    }

    public class SoftwareInventory
    {
        public List<InstalledSoftware> Applications { get; set; } = new();
        public List<WindowsFeature> Features { get; set; } = new();
        public List<Driver> Drivers { get; set; } = new();
        public DateTime ScanDate { get; set; }
        
        // Additional properties for API compatibility
        public IEnumerable<InstalledSoftware> Software => Applications;
    }

    public class InstalledSoftware
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public DateTime InstallDate { get; set; }
        public string InstallLocation { get; set; } = string.Empty;
        public long Size { get; set; }
        
        // Additional properties for API compatibility
        public DateTime InstalledDate => InstallDate;
    }

    public class WindowsFeature
    {
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Driver
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateStatus
    {
        public bool UpdatesAvailable { get; set; }
        public int CriticalUpdates { get; set; }
        public int ImportantUpdates { get; set; }
        public int OptionalUpdates { get; set; }
        public List<WindowsUpdate> AvailableUpdates { get; set; } = new();
        public DateTime LastCheck { get; set; }
    }

    public class WindowsUpdate
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public long Size { get; set; }
        public string KB { get; set; } = string.Empty;
    }

    public class UpdateResult
    {
        public bool Success { get; set; }
        public int UpdatesInstalled { get; set; }
        public int UpdatesFailed { get; set; }
        public bool RestartRequired { get; set; }
        public List<string> InstalledUpdates { get; set; } = new();
        public List<string> FailedUpdates { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    public enum ServiceAction
    {
        Start,
        Stop,
        Restart,
        Enable,
        Disable
    }

    public class ServiceStatus
    {
        public bool Success { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StartType { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class RegistryBackup
    {
        public bool Success { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public long BackupSize { get; set; }
        public DateTime BackupDate { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class SystemRestore
    {
        public bool Success { get; set; }
        public string RestorePointName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Error { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public string Message => Success ? "System restore point created successfully" : Error;
        public string RestorePointId => RestorePointName;
    }

    public class PerformanceOptimization
    {
        public bool Success { get; set; }
        public List<string> OptimizationsApplied { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string Error { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public string Message => Success ? "Optimization completed successfully" : Error;
        public List<string> Optimizations => OptimizationsApplied;
    }

    public class DiskDefragmentation
    {
        public bool Success { get; set; }
        public string DriveLetter { get; set; } = string.Empty;
        public double FragmentationBefore { get; set; }
        public double FragmentationAfter { get; set; }
        public TimeSpan Duration { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class StartupOptimization
    {
        public bool Success { get; set; }
        public List<StartupItem> DisabledItems { get; set; } = new();
        public List<StartupItem> EnabledItems { get; set; } = new();
        public int ItemsOptimized { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class StartupItem
    {
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string Impact { get; set; } = string.Empty;
    }
}
