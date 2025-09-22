using System.Net;
using System.Text.RegularExpressions;
using System.Security;

namespace EnterpriseITToolkit.Security
{
    public static class SecurityValidator
    {
        private static readonly HashSet<string> PrivateIPRanges = new()
        {
            "10.0.0.0/8",
            "172.16.0.0/12",
            "192.168.0.0/16",
            "127.0.0.0/8"
        };

        private static readonly HashSet<string> ReservedIPRanges = new()
        {
            "0.0.0.0/8",
            "169.254.0.0/16",
            "224.0.0.0/4",
            "240.0.0.0/4"
        };

        public static bool IsValidIPAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            if (!IPAddress.TryParse(ip, out var ipAddress))
                return false;

            // Check if it's a private or reserved IP
            if (IsPrivateOrReserved(ip))
                return false;

            return true;
        }

        public static bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            if (hostname.Length > 253)
                return false;

            if (hostname.Contains(".."))
                return false;

            // Basic hostname validation
            var hostnameRegex = new Regex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$");
            return hostnameRegex.IsMatch(hostname);
        }

        public static bool IsValidPort(int port)
        {
            return port > 0 && port <= 65535;
        }

        public static bool IsAllowedCommand(string command)
        {
            var allowedCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ping",
                "tracert",
                "nslookup",
                "netstat",
                "ipconfig",
                "arp",
                "route"
            };

            return allowedCommands.Contains(command);
        }

        public static bool IsBlockedPort(int port)
        {
            var blockedPorts = new HashSet<int>
            {
                22,   // SSH
                23,   // Telnet
                135,  // RPC
                139,  // NetBIOS
                445,  // SMB
                3389, // RDP
                5985, // WinRM HTTP
                5986  // WinRM HTTPS
            };

            return blockedPorts.Contains(port);
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove potentially dangerous characters
            var dangerousChars = new char[] { '<', '>', '"', '\'', '&', '|', ';', '`', '$', '(', ')', '{', '}' };
            var sanitized = input;

            foreach (var c in dangerousChars)
            {
                sanitized = sanitized.Replace(c.ToString(), string.Empty);
            }

            return sanitized.Trim();
        }

        public static bool IsValidFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                var fullPath = Path.GetFullPath(filePath);
                return !fullPath.Contains("..") && Path.IsPathRooted(fullPath);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPrivateOrReserved(string ip)
        {
            if (!IPAddress.TryParse(ip, out var ipAddress))
                return false;

            var ipBytes = ipAddress.GetAddressBytes();

            // Check private ranges
            if (ipBytes[0] == 10) return true; // 10.0.0.0/8
            if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) return true; // 172.16.0.0/12
            if (ipBytes[0] == 192 && ipBytes[1] == 168) return true; // 192.168.0.0/16
            if (ipBytes[0] == 127) return true; // 127.0.0.0/8

            // Check reserved ranges
            if (ipBytes[0] == 0) return true; // 0.0.0.0/8
            if (ipBytes[0] == 169 && ipBytes[1] == 254) return true; // 169.254.0.0/16
            if (ipBytes[0] >= 224) return true; // 224.0.0.0/4 and 240.0.0.0/4

            return false;
        }
    }
}
