using System.Diagnostics;
using System.Security;
using Microsoft.Extensions.Logging;

namespace EnterpriseITToolkit.Security
{
    public class ProcessResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    public static class SecureProcessRunner
    {
        private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            "ping",
            "tracert",
            "nslookup",
            "netstat",
            "ipconfig",
            "arp",
            "route",
            "systeminfo",
            "whoami",
            "hostname"
        };

        public static async Task<ProcessResult> RunSecurelyAsync(string command, string[] args, ILogger? logger = null)
        {
            var result = new ProcessResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validate command
                if (!IsAllowedCommand(command))
                {
                    throw new SecurityException($"Command '{command}' is not allowed");
                }

                // Validate arguments
                var sanitizedArgs = SanitizeArguments(args);

                logger?.LogInformation("Executing secure command: {Command} with args: {Args}", command, string.Join(" ", sanitizedArgs));

                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = string.Join(" ", sanitizedArgs),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                process.Start();
                
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                result.Output = await outputTask;
                result.Error = await errorTask;
                result.ExitCode = process.ExitCode;
                result.Success = process.ExitCode == 0;

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;

                logger?.LogInformation("Command completed with exit code: {ExitCode} in {ExecutionTime}ms", 
                    process.ExitCode, result.ExecutionTime.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                result.Success = false;
                result.Error = ex.Message;
                result.ExitCode = -1;

                logger?.LogError(ex, "Error executing command: {Command}", command);
                return result;
            }
        }

        public static ProcessResult RunSecurely(string command, string[] args, ILogger? logger = null)
        {
            return RunSecurelyAsync(command, args, logger).GetAwaiter().GetResult();
        }

        private static bool IsAllowedCommand(string command)
        {
            return AllowedCommands.Contains(command.ToLowerInvariant());
        }

        private static string[] SanitizeArguments(string[] args)
        {
            if (args == null || args.Length == 0)
                return Array.Empty<string>();

            var sanitized = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                sanitized[i] = SecurityValidator.SanitizeInput(args[i]);
            }

            return sanitized;
        }
    }
}
