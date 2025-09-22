namespace EnterpriseITToolkit.Services
{
    public interface IWindows11Service
    {
        Task<CompatibilityResult> CheckCompatibilityAsync();
        Task<TPMStatus> CheckTPMAsync();
        Task<SecureBootStatus> CheckSecureBootAsync();
        Task<RAMStatus> CheckRAMAsync();
        Task<BackupResult> BackupProfileAsync(string backupPath);
    }

    public class CompatibilityResult
    {
        public bool IsCompatible { get; set; }
        public TPMStatus TPM { get; set; } = new();
        public SecureBootStatus SecureBoot { get; set; } = new();
        public RAMStatus RAM { get; set; } = new();
        public List<string> Issues { get; set; } = new();
    }

    public class TPMStatus
    {
        public bool IsPresent { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
    }

    public class SecureBootStatus
    {
        public bool IsEnabled { get; set; }
        public string Policy { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }

    public class RAMStatus
    {
        public long TotalRAMBytes { get; set; }
        public long AvailableRAMBytes { get; set; }
        public string RAMType { get; set; } = string.Empty;
        public bool MeetsRequirements { get; set; }
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public long BackupSizeBytes { get; set; }
        public List<string> BackedUpItems { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }
}
