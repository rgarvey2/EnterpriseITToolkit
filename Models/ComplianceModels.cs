using System.ComponentModel.DataAnnotations;

namespace EnterpriseITToolkit.Models
{
    public class ComplianceRule
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        [MaxLength(100)]
        public string? Category { get; set; }
        [MaxLength(100)]
        public string? Standard { get; set; }
        [MaxLength(50)]
        public string? Severity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
    }

    public class ComplianceCheck
    {
        public int Id { get; set; }
        public int ComplianceRuleId { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Message { get; set; }
        [MaxLength(200)]
        public string? ComputerName { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? CheckedBy { get; set; }
        public bool IsCompliant { get; set; } = false;

        // Navigation Properties
        public virtual ComplianceRule ComplianceRule { get; set; } = null!;
    }

    public class Asset
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string AssetTag { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? AssetType { get; set; }
        [MaxLength(100)]
        public string? Manufacturer { get; set; }
        [MaxLength(100)]
        public string? Model { get; set; }
        [MaxLength(100)]
        public string? SerialNumber { get; set; }
        [MaxLength(200)]
        public string? Location { get; set; }
        [MaxLength(100)]
        public string? AssignedTo { get; set; }
        [MaxLength(100)]
        public string? Department { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }

    public class License
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? LicenseKey { get; set; }
        [MaxLength(100)]
        public string? Vendor { get; set; }
        [MaxLength(50)]
        public string? Version { get; set; }
        [MaxLength(50)]
        public string? LicenseType { get; set; }
        [MaxLength(100)]
        public string? AssignedTo { get; set; }
        [MaxLength(200)]
        public string? ComputerName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? Seats { get; set; }
        public int? UsedSeats { get; set; }
        public decimal? Cost { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }

    public class AutomationTask
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        [MaxLength(100)]
        public string? TaskType { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public bool IsEnabled { get; set; } = true;
        [MaxLength(2000)]
        public string? ScriptContent { get; set; }
        [MaxLength(500)]
        public string? Parameters { get; set; }

        // Navigation Properties
        public virtual ICollection<TaskExecution> TaskExecutions { get; set; } = new List<TaskExecution>();
    }

    public class ScheduledJob
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }
        [MaxLength(100)]
        public string? JobType { get; set; }
        [MaxLength(100)]
        public string? CronExpression { get; set; }
        [MaxLength(50)]
        public string? Status { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public bool IsEnabled { get; set; } = true;
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(2000)]
        public string? Configuration { get; set; }
    }

    public class TaskExecution
    {
        public int Id { get; set; }
        public int AutomationTaskId { get; set; }
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        [MaxLength(2000)]
        public string? Output { get; set; }
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }
        [MaxLength(100)]
        public string? ExecutedBy { get; set; }
        [MaxLength(200)]
        public string? ComputerName { get; set; }

        // Navigation Properties
        public virtual AutomationTask AutomationTask { get; set; } = null!;
    }
}
