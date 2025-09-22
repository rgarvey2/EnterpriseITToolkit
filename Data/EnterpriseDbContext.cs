using Microsoft.EntityFrameworkCore;
using EnterpriseITToolkit.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseITToolkit.Data
{
    public class EnterpriseDbContext : DbContext
    {
        public EnterpriseDbContext(DbContextOptions<EnterpriseDbContext> options) : base(options)
        {
        }

        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        // System Management
        public DbSet<SystemInfo> SystemInfos { get; set; }
        public DbSet<NetworkDevice> NetworkDevices { get; set; }
        public DbSet<SoftwareInventory> SoftwareInventories { get; set; }
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Automation & Tasks
        public DbSet<AutomationTask> AutomationTasks { get; set; }
        public DbSet<ScheduledJob> ScheduledJobs { get; set; }
        public DbSet<TaskExecution> TaskExecutions { get; set; }

        // Monitoring & Metrics
        public DbSet<SystemMetric> SystemMetrics { get; set; }
        public DbSet<PerformanceAlert> PerformanceAlerts { get; set; }
        public DbSet<HealthCheck> HealthChecks { get; set; }

        // Compliance & Governance
        public DbSet<ComplianceRule> ComplianceRules { get; set; }
        public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<License> Licenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Management Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Many-to-Many Relationships
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });
                entity.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId);
                entity.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId);
            });

            // System Management Configuration
            modelBuilder.Entity<SystemInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ComputerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OperatingSystem).HasMaxLength(200);
                entity.Property(e => e.Processor).HasMaxLength(200);
                entity.Property(e => e.Memory).HasMaxLength(100);
                entity.HasIndex(e => e.ComputerName);
            });

            modelBuilder.Entity<NetworkDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.Hostname).HasMaxLength(255);
                entity.Property(e => e.MacAddress).HasMaxLength(17);
                entity.Property(e => e.DeviceType).HasMaxLength(100);
                entity.HasIndex(e => e.IpAddress);
            });

            // Audit Logging Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Resource).HasMaxLength(200);
                entity.Property(e => e.Details).HasMaxLength(2000);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
            });

            // Security Events Configuration
            modelBuilder.Entity<SecurityEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Source).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.Severity);
            });

            // Performance Monitoring Configuration
            modelBuilder.Entity<SystemMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MetricName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ComputerName).HasMaxLength(100);
                entity.Property(e => e.Value).HasColumnType("decimal(18,4)");
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.MetricName);
            });

            // Compliance Configuration
            modelBuilder.Entity<ComplianceRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Standard).HasMaxLength(100);
            });

            // Asset Management Configuration
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssetTag).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AssetType).HasMaxLength(100);
                entity.Property(e => e.Manufacturer).HasMaxLength(100);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.HasIndex(e => e.AssetTag).IsUnique();
            });

            // License Management Configuration
            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LicenseKey).HasMaxLength(500);
                entity.Property(e => e.Vendor).HasMaxLength(100);
                entity.Property(e => e.Version).HasMaxLength(50);
                entity.Property(e => e.LicenseType).HasMaxLength(50);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.HasIndex(e => e.ProductName);
            });
        }
    }
}
