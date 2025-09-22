using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public interface IBackgroundJobService
    {
        Task<string> ScheduleJobAsync(BackgroundJob job);
        Task<bool> CancelJobAsync(string jobId);
        Task<BackgroundJob?> GetJobAsync(string jobId);
        Task<List<BackgroundJob>> GetJobsAsync(JobStatus? status = null);
        Task<List<BackgroundJob>> GetJobsByTypeAsync(string jobType);
        Task<bool> UpdateJobStatusAsync(string jobId, JobStatus status, string? message = null);
        Task<List<BackgroundJob>> GetJobsByUserAsync(string userId);
        Task<bool> RetryJobAsync(string jobId);
        Task<bool> DeleteJobAsync(string jobId);
        Task<JobStatistics> GetJobStatisticsAsync();
        Task<List<BackgroundJob>> GetFailedJobsAsync();
        Task<bool> PauseJobAsync(string jobId);
        Task<bool> ResumeJobAsync(string jobId);
        Task<bool> ScheduleRecurringJobAsync(RecurringJob recurringJob);
        Task<bool> CancelRecurringJobAsync(string recurringJobId);
        Task<List<RecurringJob>> GetRecurringJobsAsync();
    }

    public class BackgroundJob
    {
        public string Id { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public JobPriority Priority { get; set; } = JobPriority.Normal;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? AssignedTo { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public Dictionary<string, object> Results { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public TimeSpan? Timeout { get; set; }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class RecurringJob
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
        public string? CreatedBy { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public enum JobStatus
    {
        Pending,
        Scheduled,
        Running,
        Completed,
        Failed,
        Cancelled,
        Paused
    }

    public enum JobPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public class JobStatistics
    {
        public int TotalJobs { get; set; }
        public int PendingJobs { get; set; }
        public int RunningJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int CancelledJobs { get; set; }
        public double AverageExecutionTime { get; set; }
        public double SuccessRate { get; set; }
        public Dictionary<string, int> JobsByType { get; set; } = new();
        public Dictionary<string, int> JobsByStatus { get; set; } = new();
    }
}
