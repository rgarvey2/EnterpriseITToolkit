using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkController : BaseApiController
    {
        private readonly INetworkService _networkService;

        public NetworkController(
            ILogger<NetworkController> logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService,
            INetworkService networkService)
            : base(logger, auditService, authService)
        {
            _networkService = networkService;
        }

        [HttpPost("ping")]
        public async Task<IActionResult> Ping([FromBody] PingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Host))
                {
                    return BadRequest(new { error = "Host is required" });
                }

                var result = await _networkService.PingAsync(request.Host, request.Timeout);
                
                await LogAuditEventAsync("NETWORK_PING", "Network", $"Ping to {request.Host}", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    host = result.Host,
                    ipAddress = result.IpAddress,
                    roundTripTime = result.RoundTripTime,
                    status = result.Status,
                    attempts = result.Attempts,
                    executionTime = result.ExecutionTime
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Ping");
            }
        }

        [HttpPost("traceroute")]
        public async Task<IActionResult> Traceroute([FromBody] TracerouteRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Host))
                {
                    return BadRequest(new { error = "Host is required" });
                }

                var result = await _networkService.TraceRouteAsync(request.Host, request.MaxHops);
                
                await LogAuditEventAsync("NETWORK_TRACEROUTE", "Network", $"Traceroute to {request.Host}", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    host = result.Host,
                    ipAddress = result.IpAddress,
                    hops = result.Hops.Select(h => new
                    {
                        hopNumber = h.HopNumber,
                        ipAddress = h.IpAddress,
                        hostname = h.Hostname,
                        roundTripTime = h.RoundTripTime,
                        status = h.Status
                    }),
                    executionTime = result.ExecutionTime
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Traceroute");
            }
        }

        [HttpPost("port-scan")]
        public async Task<IActionResult> PortScan([FromBody] PortScanRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Host))
                {
                    return BadRequest(new { error = "Host is required" });
                }

                var result = await _networkService.ScanPortsAsync(request.Host, request.Ports, request.Timeout);
                
                await LogAuditEventAsync("NETWORK_PORT_SCAN", "Network", $"Port scan on {request.Host}", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    host = result.Host,
                    openPorts = result.OpenPorts.Select(p => new
                    {
                        port = p.Port,
                        service = p.Service,
                        protocol = p.Protocol
                    }),
                    executionTime = result.ExecutionTime
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PortScan");
            }
        }

        [HttpPost("dns-lookup")]
        public async Task<IActionResult> DnsLookup([FromBody] DnsLookupRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Hostname))
                {
                    return BadRequest(new { error = "Hostname is required" });
                }

                var result = await _networkService.DnsLookupAsync(request.Hostname, request.RecordType);
                
                await LogAuditEventAsync("NETWORK_DNS_LOOKUP", "Network", $"DNS lookup for {request.Hostname}", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    hostname = result.Hostname,
                    recordType = result.RecordType,
                    records = result.Records.Select(r => new
                    {
                        type = r.Type,
                        value = r.Value,
                        ttl = r.Ttl
                    }),
                    executionTime = result.ExecutionTime
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DnsLookup");
            }
        }

        [HttpPost("bandwidth-test")]
        public async Task<IActionResult> BandwidthTest([FromBody] BandwidthTestRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ServerUrl))
                {
                    return BadRequest(new { error = "Server URL is required" });
                }

                var result = await _networkService.TestBandwidthAsync(request.ServerUrl, request.Duration);
                
                await LogAuditEventAsync("NETWORK_BANDWIDTH_TEST", "Network", $"Bandwidth test to {request.ServerUrl}", result.Success);
                
                return Ok(new
                {
                    success = result.Success,
                    serverUrl = result.ServerUrl,
                    downloadSpeed = result.DownloadSpeed,
                    uploadSpeed = result.UploadSpeed,
                    latency = result.Latency,
                    packetLoss = result.PacketLoss,
                    executionTime = result.ExecutionTime
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "BandwidthTest");
            }
        }

        [HttpGet("network-adapters")]
        public async Task<IActionResult> GetNetworkAdapters()
        {
            try
            {
                var adapters = await _networkService.GetNetworkAdaptersAsync();
                
                await LogAuditEventAsync("NETWORK_ADAPTERS", "Network", "Network adapters retrieved", true);
                
                return Ok(new
                {
                    success = true,
                    adapters = adapters.Select(a => new
                    {
                        name = a.Name,
                        description = a.Description,
                        status = a.Status,
                        speed = a.Speed,
                        macAddress = a.MacAddress,
                        ipAddresses = a.IpAddresses
                    })
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetNetworkAdapters");
            }
        }
    }

    public class PingRequest
    {
        public string Host { get; set; } = string.Empty;
        public int Timeout { get; set; } = 5000;
    }

    public class TracerouteRequest
    {
        public string Host { get; set; } = string.Empty;
        public int MaxHops { get; set; } = 30;
    }

    public class PortScanRequest
    {
        public string Host { get; set; } = string.Empty;
        public int[] Ports { get; set; } = { 21, 22, 23, 25, 53, 80, 110, 143, 443, 993, 995 };
        public int Timeout { get; set; } = 5000;
    }

    public class DnsLookupRequest
    {
        public string Hostname { get; set; } = string.Empty;
        public string RecordType { get; set; } = "A";
    }

    public class BandwidthTestRequest
    {
        public string ServerUrl { get; set; } = string.Empty;
        public int Duration { get; set; } = 10;
    }
}
