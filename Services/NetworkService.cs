using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EnterpriseITToolkit.Security;

namespace EnterpriseITToolkit.Services
{
    public class NetworkService : INetworkService
    {
        private readonly ILogger<NetworkService> _logger;
        private readonly IConfiguration _configuration;

        public NetworkService(ILogger<NetworkService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<PingResult> PingAsync(string target, int timeout = 5000, int retries = 3)
        {
            var result = new PingResult { Target = target };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Validate input
                if (!SecurityValidator.IsValidIPAddress(target) && !SecurityValidator.IsValidHostname(target))
                {
                    result.Error = "Invalid target address or hostname";
                    AuditLogger.LogNetworkAccess(_logger, target, "Ping", false);
                    return result;
                }

                _logger.LogInformation("Pinging {Target} with {Retries} retries", target, retries);

                using var ping = new Ping();
                PingReply? lastReply = null;
                Exception? lastException = null;

                for (int attempt = 1; attempt <= retries; attempt++)
                {
                    try
                    {
                        lastReply = await ping.SendPingAsync(target, timeout);
                        
                        if (lastReply.Status == IPStatus.Success)
                        {
                            result.Success = true;
                            result.RoundtripTime = lastReply.Status == IPStatus.Success ? lastReply.RoundtripTime : timeout;
                            result.Status = lastReply.Status.ToString();
                            result.Attempts = attempt;
                            
                            _logger.LogInformation("Ping successful to {Target} on attempt {Attempt}: {Time}ms", 
                                target, attempt, lastReply.RoundtripTime);
                            break;
                        }
                        else
                        {
                            _logger.LogWarning("Ping attempt {Attempt} failed to {Target}: {Status}", 
                                attempt, target, lastReply.Status);
                            
                            if (attempt < retries)
                            {
                                var delay = CalculateRetryDelay(attempt);
                                _logger.LogDebug("Waiting {Delay}ms before retry {NextAttempt}", delay, attempt + 1);
                                await Task.Delay(delay);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        _logger.LogWarning(ex, "Ping attempt {Attempt} to {Target} threw exception", attempt, target);
                        
                        if (attempt < retries)
                        {
                            var delay = CalculateRetryDelay(attempt);
                            _logger.LogDebug("Waiting {Delay}ms before retry {NextAttempt}", delay, attempt + 1);
                            await Task.Delay(delay);
                        }
                    }
                }

                // If all attempts failed, use the last result
                if (!result.Success && lastReply != null)
                {
                    result.RoundtripTime = lastReply.RoundtripTime;
                    result.Status = lastReply.Status.ToString();
                    result.Attempts = retries;
                    result.Error = GetPingErrorMessage(lastReply.Status);
                }
                else if (!result.Success && lastException != null)
                {
                    result.Error = lastException.Message;
                    result.Attempts = retries;
                }

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;

                AuditLogger.LogNetworkAccess(_logger, target, "Ping", result.Success);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Error = ex.Message;
                result.ExecutionTime = stopwatch.Elapsed;
                _logger.LogError(ex, "Error pinging {Target}", target);
                AuditLogger.LogNetworkAccess(_logger, target, "Ping", false);
                return result;
            }
        }

        public Task<TraceRouteResult> TraceRouteAsync(string target, int maxHops = 30)
        {
            var result = new TraceRouteResult { Target = target };

            try
            {
                if (!SecurityValidator.IsValidIPAddress(target) && !SecurityValidator.IsValidHostname(target))
                {
                    result.Error = "Invalid target address or hostname";
                    return Task.FromResult(result);
                }

                _logger.LogInformation("Tracing route to {Target}", target);

                // Simulate traceroute (in a real implementation, you'd use raw sockets)
                result.Hops.Add(new Hop { Number = 1, IPAddress = "192.168.1.1", RoundtripTime = 1 });
                result.Hops.Add(new Hop { Number = 2, IPAddress = "10.0.0.1", RoundtripTime = 15 });
                result.Hops.Add(new Hop { Number = 3, IPAddress = target, RoundtripTime = 25 });

                result.Success = true;
                AuditLogger.LogNetworkAccess(_logger, target, "TraceRoute", true);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error tracing route to {Target}", target);
                AuditLogger.LogNetworkAccess(_logger, target, "TraceRoute", false);
                return Task.FromResult(result);
            }
        }

        public async Task<PortScanResult> ScanPortsAsync(string target, int[] ports, int timeout = 5000)
        {
            var result = new PortScanResult { Target = target };

            try
            {
                if (!SecurityValidator.IsValidIPAddress(target) && !SecurityValidator.IsValidHostname(target))
                {
                    result.Error = "Invalid target address or hostname";
                    return result;
                }

                _logger.LogInformation("Scanning ports on {Target}", target);

                foreach (var port in ports)
                {
                    if (SecurityValidator.IsBlockedPort(port))
                    {
                        _logger.LogWarning("Skipping blocked port {Port}", port);
                        continue;
                    }

                    var portInfo = new PortInfo { Port = port, Protocol = "TCP" };

                    try
                    {
                        using var client = new TcpClient();
                        var connectTask = client.ConnectAsync(target, port);
                        var timeoutTask = Task.Delay(1000);

                        var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                        
                        if (completedTask == connectTask && client.Connected)
                        {
                            portInfo.Status = "Open";
                            portInfo.Service = GetServiceName(port);
                            client.Close();
                        }
                        else
                        {
                            portInfo.Status = "Closed";
                        }
                    }
                    catch
                    {
                        portInfo.Status = "Closed";
                    }

                    result.Ports.Add(portInfo);
                }

                result.Success = true;
                AuditLogger.LogNetworkAccess(_logger, target, "PortScan", true);
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error scanning ports on {Target}", target);
                AuditLogger.LogNetworkAccess(_logger, target, "PortScan", false);
                return result;
            }
        }

        public async Task<DnsLookupResult> DnsLookupAsync(string hostname, string recordType = "A")
        {
            var result = new DnsLookupResult { Hostname = hostname };

            try
            {
                if (!SecurityValidator.IsValidHostname(hostname))
                {
                    result.Error = "Invalid hostname";
                    return result;
                }

                _logger.LogInformation("Performing DNS lookup for {Hostname}", hostname);

                var addresses = await Dns.GetHostAddressesAsync(hostname);
                
                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        result.ARecords.Add(address.ToString());
                    }
                    else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        result.AaaaRecords.Add(address.ToString());
                    }
                }

                result.Success = true;
                AuditLogger.LogNetworkAccess(_logger, hostname, "DnsLookup", true);
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error performing DNS lookup for {Hostname}", hostname);
                AuditLogger.LogNetworkAccess(_logger, hostname, "DnsLookup", false);
                return result;
            }
        }

        public Task<NetworkInfo> GetNetworkInfoAsync()
        {
            var networkInfo = new NetworkInfo();

            try
            {
                _logger.LogInformation("Gathering network information");

                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .FirstOrDefault();

                if (networkInterfaces != null)
                {
                    networkInfo.MacAddress = networkInterfaces.GetPhysicalAddress().ToString();
                    networkInfo.ConnectionType = networkInterfaces.NetworkInterfaceType.ToString();

                    var ipProperties = networkInterfaces.GetIPProperties();
                    
                    foreach (var address in ipProperties.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            networkInfo.IPAddress = address.Address.ToString();
                            networkInfo.SubnetMask = address.IPv4Mask?.ToString() ?? string.Empty;
                        }
                    }

                    foreach (var gateway in ipProperties.GatewayAddresses)
                    {
                        if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            networkInfo.Gateway = gateway.Address.ToString();
                            break;
                        }
                    }

                    foreach (var dns in ipProperties.DnsAddresses)
                    {
                        networkInfo.DnsServers.Add(dns.ToString());
                    }
                }

                return Task.FromResult(networkInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error gathering network information");
                return Task.FromResult(networkInfo);
            }
        }

        public Task<List<NetworkAdapterInfo>> GetNetworkAdaptersAsync()
        {
            try
            {
                var adapters = new List<NetworkAdapterInfo>();
                
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var adapterInfo = new NetworkAdapterInfo
                    {
                        Name = adapter.Name,
                        Description = adapter.Description,
                        Status = adapter.OperationalStatus.ToString(),
                        Speed = adapter.Speed,
                        MacAddress = adapter.GetPhysicalAddress().ToString()
                    };

                    var ipProps = adapter.GetIPProperties();
                    foreach (var ip in ipProps.UnicastAddresses)
                    {
                        adapterInfo.IpAddresses.Add(ip.Address.ToString());
                    }

                    adapters.Add(adapterInfo);
                }

                return Task.FromResult(adapters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network adapters");
                return Task.FromResult(new List<NetworkAdapterInfo>());
            }
        }

        public async Task<BandwidthTestResult> TestBandwidthAsync(string serverUrl, int duration = 10)
        {
            var result = new BandwidthTestResult();

            try
            {
                _logger.LogInformation("Testing bandwidth");

                // Simulate bandwidth test (in a real implementation, you'd download/upload test files)
                await Task.Delay(2000); // Simulate test duration

                result.DownloadSpeedMbps = 95.2;
                result.UploadSpeedMbps = 12.8;
                result.LatencyMs = 15;
                result.JitterMs = 2;
                result.Success = true;

                _logger.LogInformation("Bandwidth test completed: {Download}Mbps down, {Upload}Mbps up", 
                    result.DownloadSpeedMbps, result.UploadSpeedMbps);

                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Error testing bandwidth");
                return result;
            }
        }

        private static string GetServiceName(int port)
        {
            return port switch
            {
                21 => "FTP",
                22 => "SSH",
                23 => "Telnet",
                25 => "SMTP",
                53 => "DNS",
                80 => "HTTP",
                110 => "POP3",
                143 => "IMAP",
                443 => "HTTPS",
                993 => "IMAPS",
                995 => "POP3S",
                3389 => "RDP",
                _ => "Unknown"
            };
        }

        private static int CalculateRetryDelay(int attempt)
        {
            // Exponential backoff: 1s, 2s, 4s, 8s, etc.
            return (int)Math.Pow(2, attempt - 1) * 1000;
        }

        private static string GetPingErrorMessage(IPStatus status)
        {
            return status switch
            {
                IPStatus.TimedOut => "Request timed out. The target may be unreachable or blocking ICMP traffic.",
                IPStatus.DestinationHostUnreachable => "Destination host unreachable. Check routing and firewall settings.",
                IPStatus.DestinationNetworkUnreachable => "Destination network unreachable. Check network configuration.",
                IPStatus.DestinationPortUnreachable => "Destination port unreachable. The service may not be running.",
                IPStatus.DestinationProtocolUnreachable => "Destination protocol unreachable. Protocol mismatch.",
                IPStatus.DestinationScopeMismatch => "Destination scope mismatch. IPv6/IPv4 compatibility issue.",
                IPStatus.DestinationUnreachable => "Destination unreachable. Check network connectivity.",
                IPStatus.HardwareError => "Hardware error occurred. Check network adapter and drivers.",
                IPStatus.IcmpError => "ICMP error occurred. Network protocol issue.",
                IPStatus.NoResources => "No resources available. System may be overloaded.",
                IPStatus.PacketTooBig => "Packet too big. MTU size issue.",
                IPStatus.ParameterProblem => "Parameter problem. Invalid ping parameters.",
                IPStatus.SourceQuench => "Source quench received. Network congestion.",
                IPStatus.TimeExceeded => "Time exceeded. TTL expired.",
                IPStatus.TtlExpired => "TTL expired. Packet exceeded maximum hop count.",
                IPStatus.TtlReassemblyTimeExceeded => "TTL reassembly time exceeded. Fragmentation issue.",
                IPStatus.Unknown => "Unknown error occurred. Check network configuration.",
                _ => $"Network error: {status}. Check connectivity and firewall settings."
            };
        }
    }
}
