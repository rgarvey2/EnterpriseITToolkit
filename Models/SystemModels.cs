using System.ComponentModel.DataAnnotations;

namespace EnterpriseITToolkit.Models
{
    public class SystemInfo
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string ComputerName { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? OperatingSystem { get; set; }
        [MaxLength(200)]
        public string? Processor { get; set; }
        [MaxLength(100)]
        public string? Memory { get; set; }
        [MaxLength(100)]
        public string? DiskSpace { get; set; }
        [MaxLength(100)]
        public string? NetworkAdapter { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; } = true;
    }

    public class NetworkDevice
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Hostname { get; set; }
        [MaxLength(17)]
        public string? MacAddress { get; set; }
        [MaxLength(100)]
        public string? DeviceType { get; set; }
        [MaxLength(100)]
        public string? Manufacturer { get; set; }
        [MaxLength(200)]
        public string? Description { get; set; }
        public bool IsOnline { get; set; } = false;
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    }

    public class SoftwareInventory
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Version { get; set; }
        [MaxLength(100)]
        public string? Publisher { get; set; }
        [MaxLength(100)]
        public string? ComputerName { get; set; }
        public DateTime InstalledDate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class SecurityEvent
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Source { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        [MaxLength(100)]
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        [MaxLength(100)]
        public string? ResolvedBy { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string? UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Resource { get; set; }
        [MaxLength(2000)]
        public string? Details { get; set; }
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; } = true;
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }
    }

    public class SystemMetric
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string MetricName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? ComputerName { get; set; }
        public decimal Value { get; set; }
        [MaxLength(50)]
        public string? Unit { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [MaxLength(200)]
        public string? Description { get; set; }
    }

    public class PerformanceAlert
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string AlertType { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? ComputerName { get; set; }
        [MaxLength(200)]
        public string? MetricName { get; set; }
        public decimal Threshold { get; set; }
        public decimal ActualValue { get; set; }
        [MaxLength(1000)]
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsAcknowledged { get; set; } = false;
        public DateTime? AcknowledgedAt { get; set; }
        [MaxLength(100)]
        public string? AcknowledgedBy { get; set; }
    }

    public class HealthCheck
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string CheckName { get; set; } = string.Empty;
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public TimeSpan Duration { get; set; }
        [MaxLength(200)]
        public string? ComputerName { get; set; }
    }
}
