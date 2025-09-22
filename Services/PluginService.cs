using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace EnterpriseITToolkit.Services
{
    public interface IPluginService
    {
        Task<List<PluginInfo>> LoadPluginsAsync(string pluginDirectory);
        Task<PluginResult> ExecutePluginAsync(string pluginName, Dictionary<string, object> parameters);
        Task<bool> UnloadPluginAsync(string pluginName);
        List<PluginInfo> GetLoadedPlugins();
        bool IsPluginLoaded(string pluginName);
    }

    public class PluginService : IPluginService
    {
        private readonly ILogger<PluginService> _logger;
        private readonly ICorrelationService _correlationService;
        private readonly Dictionary<string, IPlugin> _loadedPlugins;
        private readonly Dictionary<string, AssemblyLoadContext> _pluginContexts;

        public PluginService(ILogger<PluginService> logger, ICorrelationService correlationService)
        {
            _logger = logger;
            _correlationService = correlationService;
            _loadedPlugins = new Dictionary<string, IPlugin>();
            _pluginContexts = new Dictionary<string, AssemblyLoadContext>();
        }

        public async Task<List<PluginInfo>> LoadPluginsAsync(string pluginDirectory)
        {
            using var scope = _correlationService.CreateCorrelationScope("LoadPlugins");
            var loadedPlugins = new List<PluginInfo>();

            try
            {
                if (!Directory.Exists(pluginDirectory))
                {
                    _logger.LogWarning("Plugin directory does not exist: {PluginDirectory}", pluginDirectory);
                    return loadedPlugins;
                }

                _correlationService.LogWithCorrelation(LogLevel.Information, 
                    "Loading plugins from directory: {PluginDirectory}", pluginDirectory);

                var pluginFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories);

                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        var pluginInfo = await LoadPluginFromFileAsync(pluginFile);
                        if (pluginInfo != null)
                        {
                            loadedPlugins.Add(pluginInfo);
                            _correlationService.LogWithCorrelation(LogLevel.Information, 
                                "Successfully loaded plugin: {PluginName} v{Version}", 
                                pluginInfo.Name, pluginInfo.Version);
                        }
                    }
                    catch (Exception ex)
                    {
                        _correlationService.LogWithCorrelation(LogLevel.Warning, ex, 
                            "Failed to load plugin from file: {PluginFile}", pluginFile);
                    }
                }

                _correlationService.LogWithCorrelation(LogLevel.Information, 
                    "Plugin loading completed. Loaded {Count} plugins", loadedPlugins.Count);

                return loadedPlugins;
            }
            catch (Exception ex)
            {
                _correlationService.LogWithCorrelation(LogLevel.Error, ex, 
                    "Error loading plugins from directory: {PluginDirectory}", pluginDirectory);
                return loadedPlugins;
            }
        }

        private Task<PluginInfo?> LoadPluginFromFileAsync(string pluginFile)
        {
            try
            {
                var context = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(pluginFile), true);
                var assembly = context.LoadFromAssemblyPath(pluginFile);

                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                if (!pluginTypes.Any())
                {
                    _logger.LogWarning("No plugin types found in assembly: {PluginFile}", pluginFile);
                    return Task.FromResult<PluginInfo?>(null);
                }

                var pluginType = pluginTypes.First();
                var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;

                var pluginInfo = new PluginInfo
                {
                    Name = plugin.Name,
                    Version = plugin.Version,
                    Description = plugin.Description,
                    Author = plugin.Author,
                    FilePath = pluginFile,
                    LoadedAt = DateTime.UtcNow,
                    SupportedOperations = plugin.GetSupportedOperations()
                };

                _loadedPlugins[plugin.Name] = plugin;
                _pluginContexts[plugin.Name] = context;

                return Task.FromResult<PluginInfo?>(pluginInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plugin from file: {PluginFile}", pluginFile);
                return Task.FromResult<PluginInfo?>(null);
            }
        }

        public Task<PluginResult> ExecutePluginAsync(string pluginName, Dictionary<string, object> parameters)
        {
            using var scope = _correlationService.CreateCorrelationScope($"ExecutePlugin_{pluginName}");
            
            try
            {
                if (!_loadedPlugins.TryGetValue(pluginName, out var plugin))
                {
                    return Task.FromResult(new PluginResult
                    {
                        Success = false,
                        Error = $"Plugin '{pluginName}' is not loaded",
                        ExecutionTime = TimeSpan.Zero
                    });
                }

                _correlationService.LogWithCorrelation(LogLevel.Information, 
                    "Executing plugin: {PluginName} with {ParameterCount} parameters", 
                    pluginName, parameters.Count);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = plugin.ExecuteAsync(parameters).GetAwaiter().GetResult();
                stopwatch.Stop();

                result.ExecutionTime = stopwatch.Elapsed;

                _correlationService.LogWithCorrelation(LogLevel.Information, 
                    "Plugin execution completed: {PluginName}, Success: {Success}, Time: {Time}ms", 
                    pluginName, result.Success, stopwatch.ElapsedMilliseconds);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _correlationService.LogWithCorrelation(LogLevel.Error, ex, 
                    "Error executing plugin: {PluginName}", pluginName);
                
                return Task.FromResult(new PluginResult
                {
                    Success = false,
                    Error = ex.Message,
                    ExecutionTime = TimeSpan.Zero
                });
            }
        }

        public Task<bool> UnloadPluginAsync(string pluginName)
        {
            try
            {
                if (!_loadedPlugins.ContainsKey(pluginName))
                {
                    _logger.LogWarning("Plugin '{PluginName}' is not loaded", pluginName);
                    return Task.FromResult(false);
                }

                _loadedPlugins.Remove(pluginName);

                if (_pluginContexts.TryGetValue(pluginName, out var context))
                {
                    context.Unload();
                    _pluginContexts.Remove(pluginName);
                }

                _logger.LogInformation("Successfully unloaded plugin: {PluginName}", pluginName);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unloading plugin: {PluginName}", pluginName);
                return Task.FromResult(false);
            }
        }

        public List<PluginInfo> GetLoadedPlugins()
        {
            return _loadedPlugins.Values.Select(plugin => new PluginInfo
            {
                Name = plugin.Name,
                Version = plugin.Version,
                Description = plugin.Description,
                Author = plugin.Author,
                LoadedAt = DateTime.UtcNow,
                SupportedOperations = plugin.GetSupportedOperations()
            }).ToList();
        }

        public bool IsPluginLoaded(string pluginName)
        {
            return _loadedPlugins.ContainsKey(pluginName);
        }
    }

    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        string Author { get; }
        Task<PluginResult> ExecuteAsync(Dictionary<string, object> parameters);
        bool CanExecute(string operation);
        List<string> GetSupportedOperations();
    }

    public class PluginResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }

    public class PluginInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime LoadedAt { get; set; }
        public List<string> SupportedOperations { get; set; } = new();
    }

    // Example plugin implementations
    public class SystemInfoPlugin : IPlugin
    {
        public string Name => "System Information Plugin";
        public string Version => "1.0.0";
        public string Description => "Provides detailed system information";
        public string Author => "Enterprise IT Toolkit";

        public Task<PluginResult> ExecuteAsync(Dictionary<string, object> parameters)
        {
            var result = new PluginResult();

            try
            {
                var systemInfo = new
                {
                    MachineName = Environment.MachineName,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    UserName = Environment.UserName,
                    UserDomainName = Environment.UserDomainName,
                    SystemDirectory = Environment.SystemDirectory,
                    CurrentDirectory = Environment.CurrentDirectory,
                    TickCount = Environment.TickCount
                };

                result.Success = true;
                result.Output = "System information gathered successfully";
                result.Data["SystemInfo"] = systemInfo;

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return Task.FromResult(result);
            }
        }

        public bool CanExecute(string operation)
        {
            return operation.Equals("GetSystemInfo", StringComparison.OrdinalIgnoreCase);
        }

        public List<string> GetSupportedOperations()
        {
            return new List<string> { "GetSystemInfo" };
        }
    }

    public class NetworkDiagnosticsPlugin : IPlugin
    {
        public string Name => "Network Diagnostics Plugin";
        public string Version => "1.0.0";
        public string Description => "Advanced network diagnostic tools";
        public string Author => "Enterprise IT Toolkit";

        public async Task<PluginResult> ExecuteAsync(Dictionary<string, object> parameters)
        {
            var result = new PluginResult();

            try
            {
                if (!parameters.TryGetValue("operation", out var operationObj) || 
                    operationObj is not string operation)
                {
                    result.Success = false;
                    result.Error = "Operation parameter is required";
                    return result;
                }

                switch (operation.ToLowerInvariant())
                {
                    case "ping":
                        result = await ExecutePingAsync(parameters);
                        break;
                    case "traceroute":
                        result = await ExecuteTracerouteAsync(parameters);
                        break;
                    default:
                        result.Success = false;
                        result.Error = $"Unsupported operation: {operation}";
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        private async Task<PluginResult> ExecutePingAsync(Dictionary<string, object> parameters)
        {
            var result = new PluginResult();

            if (!parameters.TryGetValue("target", out var targetObj) || targetObj is not string target)
            {
                result.Success = false;
                result.Error = "Target parameter is required for ping operation";
                return result;
            }

            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(target, 5000);

                result.Success = reply.Status == System.Net.NetworkInformation.IPStatus.Success;
                result.Output = $"Ping to {target}: {reply.Status}";
                result.Data["RoundtripTime"] = reply.RoundtripTime;
                result.Data["Status"] = reply.Status.ToString();

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        private async Task<PluginResult> ExecuteTracerouteAsync(Dictionary<string, object> parameters)
        {
            var result = new PluginResult();

            if (!parameters.TryGetValue("target", out var targetObj) || targetObj is not string target)
            {
                result.Success = false;
                result.Error = "Target parameter is required for traceroute operation";
                return result;
            }

            try
            {
                // Simplified traceroute implementation
                var hops = new List<object>();
                
                for (int ttl = 1; ttl <= 10; ttl++)
                {
                    using var ping = new System.Net.NetworkInformation.Ping();
                    var options = new System.Net.NetworkInformation.PingOptions(ttl, true);
                    var reply = await ping.SendPingAsync(target, 5000, new byte[32], options);
                    
                    hops.Add(new
                    {
                        Hop = ttl,
                        IP = reply.Address?.ToString() ?? "*",
                        Time = reply.RoundtripTime,
                        Status = reply.Status.ToString()
                    });

                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        break;
                }

                result.Success = true;
                result.Output = $"Traceroute to {target} completed";
                result.Data["Hops"] = hops;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        public bool CanExecute(string operation)
        {
            return operation.Equals("ping", StringComparison.OrdinalIgnoreCase) ||
                   operation.Equals("traceroute", StringComparison.OrdinalIgnoreCase);
        }

        public List<string> GetSupportedOperations()
        {
            return new List<string> { "ping", "traceroute" };
        }
    }
}
