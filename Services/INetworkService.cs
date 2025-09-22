using System.Net.NetworkInformation;

namespace EnterpriseITToolkit.Services
{
    public interface INetworkService
    {
        Task<PingResult> PingAsync(string target, int timeout = 5000, int retries = 3);
        Task<TraceRouteResult> TraceRouteAsync(string target, int maxHops = 30);
        Task<PortScanResult> ScanPortsAsync(string target, int[] ports, int timeout = 5000);
        Task<DnsLookupResult> DnsLookupAsync(string hostname, string recordType = "A");
        Task<NetworkInfo> GetNetworkInfoAsync();
        Task<BandwidthTestResult> TestBandwidthAsync(string serverUrl, int duration = 10);
        Task<List<NetworkAdapterInfo>> GetNetworkAdaptersAsync();
    }

    public class PingResult
    {
        public bool Success { get; set; }
        public string Target { get; set; } = string.Empty;
        public long RoundtripTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        
        // Additional properties for API compatibility
        public string Host => Target;
        public string IpAddress => Target; // This would be resolved in actual implementation
        public long RoundTripTime => RoundtripTime;
    }

    public class TraceRouteResult
    {
        public bool Success { get; set; }
        public string Target { get; set; } = string.Empty;
        public List<Hop> Hops { get; set; } = new();
        public string Error { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        
        // Additional properties for API compatibility
        public string Host => Target;
        public string IpAddress => Target; // This would be resolved in actual implementation
    }

    public class Hop
    {
        public int Number { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public long RoundtripTime { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public int HopNumber => Number;
        public string IpAddress => IPAddress;
        public long RoundTripTime => RoundtripTime;
    }

    public class PortScanResult
    {
        public bool Success { get; set; }
        public string Target { get; set; } = string.Empty;
        public List<PortInfo> Ports { get; set; } = new();
        public string Error { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public string Host => Target;
        public List<PortInfo> OpenPorts => Ports.Where(p => p.IsOpen).ToList();
        public TimeSpan ExecutionTime { get; set; }
    }

    public class PortInfo
    {
        public int Port { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
    }

    public class DnsLookupResult
    {
        public bool Success { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public List<string> ARecords { get; set; } = new();
        public List<string> AaaaRecords { get; set; } = new();
        public List<string> MxRecords { get; set; } = new();
        public string Error { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public string RecordType { get; set; } = "A";
        public List<DnsRecord> Records => ARecords.Select(ip => new DnsRecord { Type = "A", Value = ip, Ttl = 300 }).ToList();
        public TimeSpan ExecutionTime { get; set; }
    }
    
    public class DnsRecord
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Ttl { get; set; }
    }

    public class NetworkInfo
    {
        public string IPAddress { get; set; } = string.Empty;
        public string SubnetMask { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public List<string> DnsServers { get; set; } = new();
        public string MacAddress { get; set; } = string.Empty;
        public string ConnectionType { get; set; } = string.Empty;
    }

    public class BandwidthTestResult
    {
        public bool Success { get; set; }
        public double DownloadSpeedMbps { get; set; }
        public double UploadSpeedMbps { get; set; }
        public long LatencyMs { get; set; }
        public long JitterMs { get; set; }
        public string Error { get; set; } = string.Empty;
        
        // Additional properties for API compatibility
        public string ServerUrl { get; set; } = string.Empty;
        public double DownloadSpeed => DownloadSpeedMbps;
        public double UploadSpeed => UploadSpeedMbps;
        public long Latency => LatencyMs;
        public double PacketLoss { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
    
    public class NetworkAdapterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long Speed { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public List<string> IpAddresses { get; set; } = new();
    }
}
