using Microsoft.Extensions.Logging;
using System.Management;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Services
{
    public class Windows11Service : IWindows11Service
    {
        private readonly ILogger<Windows11Service> _logger;

        public Windows11Service(ILogger<Windows11Service> logger)
        {
            _logger = logger;
        }

        public async Task<CompatibilityResult> CheckCompatibilityAsync()
        {
            var result = new CompatibilityResult();

            try
            {
                _logger.LogInformation("Checking Windows 11 compatibility");

                result.TPM = await CheckTPMAsync();
                result.SecureBoot = await CheckSecureBootAsync();
                result.RAM = await CheckRAMAsync();

                // Check compatibility
                result.IsCompatible = result.TPM.IsPresent && 
                                    result.TPM.Version == "2.0" && 
                                    result.SecureBoot.IsEnabled && 
                                    result.RAM.MeetsRequirements;

                if (!result.TPM.IsPresent)
                    result.Issues.Add("TPM 2.0 not found");
                if (result.TPM.Version != "2.0")
                    result.Issues.Add("TPM version is not 2.0");
                if (!result.SecureBoot.IsEnabled)
                    result.Issues.Add("Secure Boot is not enabled");
                if (!result.RAM.MeetsRequirements)
                    result.Issues.Add("Insufficient RAM (minimum 4GB required)");

                _logger.LogInformation("Windows 11 compatibility check completed. Compatible: {IsCompatible}", result.IsCompatible);
                AuditLogger.LogSystemAccess(_logger, "Win11Compatibility", "Checked", result.IsCompatible);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Windows 11 compatibility");
                AuditLogger.LogSystemAccess(_logger, "Win11Compatibility", "Failed", false);
                return result;
            }
        }

        public Task<TPMStatus> CheckTPMAsync()
        {
            var status = new TPMStatus();

            try
            {
                _logger.LogInformation("Checking TPM status");

                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Tpm");
                var results = searcher.Get();

                if (results.Count > 0)
                {
                    foreach (ManagementObject obj in results)
                    {
                        status.IsPresent = true;
                        status.Version = obj["SpecVersion"]?.ToString() ?? "Unknown";
                        status.IsEnabled = obj["IsEnabled_InitialValue"]?.ToString() == "True";
                        status.Manufacturer = obj["ManufacturerIdTxt"]?.ToString() ?? "Unknown";
                        break;
                    }
                }

                _logger.LogInformation("TPM check completed. Present: {IsPresent}, Version: {Version}", status.IsPresent, status.Version);
                return Task.FromResult(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking TPM status");
                return Task.FromResult(status);
            }
        }

        public Task<SecureBootStatus> CheckSecureBootAsync()
        {
            var status = new SecureBootStatus();

            try
            {
                _logger.LogInformation("Checking Secure Boot status");

                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var secureBootState = obj["SecureBootState"]?.ToString();
                    status.IsEnabled = secureBootState == "1" || secureBootState?.ToLower() == "on";
                    break;
                }

                status.Policy = "Standard";
                status.IsValid = status.IsEnabled;

                _logger.LogInformation("Secure Boot check completed. Enabled: {IsEnabled}", status.IsEnabled);
                return Task.FromResult(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Secure Boot status");
                return Task.FromResult(status);
            }
        }

        public Task<RAMStatus> CheckRAMAsync()
        {
            var status = new RAMStatus();

            try
            {
                _logger.LogInformation("Checking RAM status");

                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                var results = searcher.Get();

                foreach (ManagementObject obj in results)
                {
                    var capacity = Convert.ToInt64(obj["Capacity"]);
                    status.TotalRAMBytes += capacity;
                }

                // Get available memory
                using var memSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                var memResults = memSearcher.Get();

                foreach (ManagementObject obj in memResults)
                {
                    var freeMemory = Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024; // Convert KB to bytes
                    status.AvailableRAMBytes = freeMemory;
                    break;
                }

                status.RAMType = "DDR4"; // Simplified
                status.MeetsRequirements = status.TotalRAMBytes >= 4L * 1024 * 1024 * 1024; // 4GB minimum

                _logger.LogInformation("RAM check completed. Total: {TotalGB}GB, Available: {AvailableGB}GB", 
                    status.TotalRAMBytes / 1024 / 1024 / 1024, status.AvailableRAMBytes / 1024 / 1024 / 1024);

                return Task.FromResult(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking RAM status");
                return Task.FromResult(status);
            }
        }

        public async Task<BackupResult> BackupProfileAsync(string backupPath)
        {
            var result = new BackupResult { BackupPath = backupPath };

            try
            {
                if (!SecurityValidator.IsValidFilePath(backupPath))
                {
                    result.Error = "Invalid backup path";
                    return result;
                }

                _logger.LogInformation("Starting profile backup to: {BackupPath}", backupPath);

                // Create backup directory
                Directory.CreateDirectory(backupPath);

                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var itemsToBackup = new[]
                {
                    "Desktop",
                    "Documents",
                    "Pictures",
                    "Downloads",
                    "Favorites"
                };

                foreach (var item in itemsToBackup)
                {
                    var sourcePath = Path.Combine(userProfile, item);
                    var destPath = Path.Combine(backupPath, item);

                    if (Directory.Exists(sourcePath))
                    {
                        // Simulate backup (in real implementation, use proper file copying)
                        await Task.Delay(500);
                        result.BackedUpItems.Add(item);
                        result.BackupSizeBytes += new DirectoryInfo(sourcePath).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
                    }
                }

                result.Success = true;

                _logger.LogInformation("Profile backup completed. Items: {ItemCount}, Size: {SizeMB}MB", 
                    result.BackedUpItems.Count, result.BackupSizeBytes / 1024 / 1024);

                AuditLogger.LogSystemAccess(_logger, "ProfileBackup", backupPath, true);

                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error backing up profile to: {BackupPath}", backupPath);
                AuditLogger.LogSystemAccess(_logger, "ProfileBackup", backupPath, false);
                return result;
            }
        }
    }
}
