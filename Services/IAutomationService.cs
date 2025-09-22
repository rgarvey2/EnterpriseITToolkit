namespace EnterpriseITToolkit.Services
{
    public interface IAutomationService
    {
        Task<ScriptResult> RunScriptAsync(string scriptPath, string[] parameters);
        Task<BulkInstallResult> BulkInstallAsync(string[] packagePaths);
        Task<List<AutomationTask>> GetScheduledTasksAsync();
        Task<DeploymentResult> DeploySoftwareAsync(string softwareName, string[] targetComputers);
        Task<ConfigurationResult> ApplyConfigurationAsync(string configName, Dictionary<string, string> settings);
        Task<MaintenanceResult> RunMaintenanceTaskAsync(string taskName);
        Task<InventoryResult> CollectInventoryAsync(string[] targetComputers);
        Task<ComplianceResult> CheckComplianceAsync(string[] targetComputers);
    }

    public class ScriptResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    public class BulkInstallResult
    {
        public bool Success { get; set; }
        public int TotalPackages { get; set; }
        public int SuccessfulInstalls { get; set; }
        public int FailedInstalls { get; set; }
        public List<InstallResult> Results { get; set; } = new();
    }

    public class InstallResult
    {
        public string PackageName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public TimeSpan InstallationTime { get; set; }
    }

    public class AutomationTask
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime NextRun { get; set; }
        public DateTime LastRun { get; set; }
    }

    public class DeploymentResult
    {
        public bool Success { get; set; }
        public string SoftwareName { get; set; } = string.Empty;
        public int TotalTargets { get; set; }
        public int SuccessfulDeployments { get; set; }
        public int FailedDeployments { get; set; }
        public List<DeploymentStatus> DeploymentStatuses { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    public class DeploymentStatus
    {
        public string ComputerName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public DateTime DeploymentTime { get; set; }
    }

    public class ConfigurationResult
    {
        public bool Success { get; set; }
        public string ConfigurationName { get; set; } = string.Empty;
        public int SettingsApplied { get; set; }
        public int SettingsFailed { get; set; }
        public List<ConfigurationSetting> AppliedSettings { get; set; } = new();
        public List<ConfigurationSetting> FailedSettings { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    public class ConfigurationSetting
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class MaintenanceResult
    {
        public bool Success { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public List<string> CompletedSteps { get; set; } = new();
        public List<string> FailedSteps { get; set; } = new();
        public TimeSpan Duration { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class InventoryResult
    {
        public bool Success { get; set; }
        public int TotalComputers { get; set; }
        public int SuccessfulCollections { get; set; }
        public int FailedCollections { get; set; }
        public List<ComputerInventory> Inventories { get; set; } = new();
        public DateTime CollectionDate { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class ComputerInventory
    {
        public string ComputerName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public long TotalRAM { get; set; }
        public long TotalDiskSpace { get; set; }
        public List<string> InstalledSoftware { get; set; } = new();
        public List<string> NetworkAdapters { get; set; } = new();
        public DateTime LastBootTime { get; set; }
        public bool CollectionSuccess { get; set; }
        public string CollectionError { get; set; } = string.Empty;
    }

    public class ComplianceResult
    {
        public bool Success { get; set; }
        public int TotalComputers { get; set; }
        public int CompliantComputers { get; set; }
        public int NonCompliantComputers { get; set; }
        public List<ComplianceStatus> ComplianceStatuses { get; set; } = new();
        public List<ComplianceRule> Rules { get; set; } = new();
        public DateTime CheckDate { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class ComplianceStatus
    {
        public string ComputerName { get; set; } = string.Empty;
        public bool IsCompliant { get; set; }
        public int PassedRules { get; set; }
        public int FailedRules { get; set; }
        public List<string> FailedRuleNames { get; set; } = new();
        public int ComplianceScore { get; set; }
    }

    public class ComplianceRule
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string CheckType { get; set; } = string.Empty;
        public string ExpectedValue { get; set; } = string.Empty;
    }
}
