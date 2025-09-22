using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Services
{
    public class NetworkDiagnostics
    {
        private readonly ILogger<NetworkDiagnostics> _logger;

        public NetworkDiagnostics(ILogger<NetworkDiagnostics> logger)
        {
            _logger = logger;
        }

        public async Task<TraceRouteResult> PerformRealTraceRouteAsync(string target, int maxHops = 30, int timeout = 5000)
        {
            var result = new TraceRouteResult { Target = target };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!SecurityValidator.IsValidIPAddress(target) && !SecurityValidator.IsValidHostname(target))
                {
                    result.Error = "Invalid target address or hostname";
                    return result;
                }

                _logger.LogInformation("Starting real traceroute to {Target}", target);

                // Resolve hostname to IP if needed
                IPAddress? targetIP;
                if (!IPAddress.TryParse(target, out targetIP))
                {
                    var addresses = await Dns.GetHostAddressesAsync(target);
                    targetIP = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    if (targetIP == null)
                    {
                        result.Error = "Could not resolve hostname to IPv4 address";
                        return result;
                    }
                }

                using var ping = new Ping();
                
                for (int ttl = 1; ttl <= maxHops; ttl++)
                {
                    var hop = new Hop { Number = ttl };
                    
                    try
                    {
                        // Create ping options with TTL
                        var options = new PingOptions(ttl, true);
                        
                        // Send ping with TTL
                        var reply = await ping.SendPingAsync(targetIP, timeout, new byte[32], options);
                        
                        hop.IPAddress = reply.Address?.ToString() ?? "Unknown";
                        hop.RoundtripTime = reply.RoundtripTime;
                        hop.Status = reply.Status.ToString();
                        
                        // Try to get hostname for the IP
                        try
                        {
                            if (reply.Address != null)
                            {
                                var hostEntry = await Dns.GetHostEntryAsync(reply.Address);
                                hop.Hostname = hostEntry.HostName;
                            }
                            else
                            {
                                hop.Hostname = hop.IPAddress;
                            }
                        }
                        catch
                        {
                            hop.Hostname = hop.IPAddress;
                        }
                        
                        result.Hops.Add(hop);
                        
                        _logger.LogDebug("Hop {TTL}: {IP} ({Hostname}) - {Time}ms", 
                            ttl, hop.IPAddress, hop.Hostname, hop.RoundtripTime);
                        
                        // If we reached the target, we're done
                        if (reply.Status == IPStatus.Success)
                        {
                            result.Success = true;
                            break;
                        }
                        
                        // If we got a TTL expired, continue to next hop
                        if (reply.Status == IPStatus.TtlExpired)
                        {
                            continue;
                        }
                        
                        // If we got a timeout, add the hop but continue
                        if (reply.Status == IPStatus.TimedOut)
                        {
                            hop.IPAddress = "*";
                            hop.Hostname = "*";
                            hop.RoundtripTime = timeout;
                            hop.Status = "Timeout";
                            result.Hops.Add(hop);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error pinging hop {TTL}", ttl);
                        hop.IPAddress = "*";
                        hop.Hostname = "*";
                        hop.RoundtripTime = timeout;
                        hop.Status = "Error";
                        result.Hops.Add(hop);
                    }
                    
                    // Small delay between hops
                    await Task.Delay(100);
                }
                
                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                result.Success = result.Hops.Any(h => h.Status == "Success");
                
                _logger.LogInformation("Traceroute completed in {Time}ms with {Hops} hops", 
                    stopwatch.ElapsedMilliseconds, result.Hops.Count);
                
                AuditLogger.LogNetworkAccess(_logger, target, "RealTraceRoute", result.Success);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Error = ex.Message;
                result.ExecutionTime = stopwatch.Elapsed;
                _logger.LogError(ex, "Error performing traceroute to {Target}", target);
                AuditLogger.LogNetworkAccess(_logger, target, "RealTraceRoute", false);
                return result;
            }
        }

        public async Task<NetworkLatencyResult> MeasureLatencyAsync(string target, int count = 10, int timeout = 5000)
        {
            var result = new NetworkLatencyResult { Target = target };
            var times = new List<long>();

            try
            {
                if (!SecurityValidator.IsValidIPAddress(target) && !SecurityValidator.IsValidHostname(target))
                {
                    result.Error = "Invalid target address or hostname";
                    return result;
                }

                _logger.LogInformation("Measuring latency to {Target} with {Count} pings", target, count);

                using var ping = new Ping();
                
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var reply = await ping.SendPingAsync(target, timeout);
                        if (reply.Status == IPStatus.Success)
                        {
                            times.Add(reply.RoundtripTime);
                            result.SuccessfulPings++;
                        }
                        else
                        {
                            result.FailedPings++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Ping {Index} failed", i + 1);
                        result.FailedPings++;
                    }
                    
                    // Small delay between pings
                    if (i < count - 1)
                        await Task.Delay(500);
                }

                if (times.Any())
                {
                    result.MinimumLatency = times.Min();
                    result.MaximumLatency = times.Max();
                    result.AverageLatency = times.Average();
                    result.PacketLoss = (double)result.FailedPings / count * 100;
                }

                result.Success = result.SuccessfulPings > 0;
                
                _logger.LogInformation("Latency measurement completed: Avg {Avg}ms, Min {Min}ms, Max {Max}ms, Loss {Loss}%", 
                    result.AverageLatency, result.MinimumLatency, result.MaximumLatency, result.PacketLoss);
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error measuring latency to {Target}", target);
                return result;
            }
        }
    }

    public class NetworkLatencyResult
    {
        public string Target { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public int SuccessfulPings { get; set; }
        public int FailedPings { get; set; }
        public double MinimumLatency { get; set; }
        public double MaximumLatency { get; set; }
        public double AverageLatency { get; set; }
        public double PacketLoss { get; set; }
    }
}
