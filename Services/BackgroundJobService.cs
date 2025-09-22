using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EnterpriseITToolkit.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, BackgroundJob> _jobs;
        private readonly ConcurrentDictionary<string, RecurringJob> _recurringJobs;
        private readonly System.Threading.Timer _jobProcessor;
        private readonly object _lockObject = new();

        public BackgroundJobService(ILogger<BackgroundJobService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _jobs = new ConcurrentDictionary<string, BackgroundJob>();
            _recurringJobs = new ConcurrentDictionary<string, RecurringJob>();
            
            // Start job processor timer (runs every 30 seconds)
            _jobProcessor = new System.Threading.Timer(ProcessJobs, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public Task<string> ScheduleJobAsync(BackgroundJob job)
        {
            try
            {
                job.Id = Guid.NewGuid().ToString();
                job.Status = JobStatus.Pending;
                job.CreatedAt = DateTime.UtcNow;
                
                if (job.ScheduledAt == null)
                {
                    job.ScheduledAt = DateTime.UtcNow;
                }

                _jobs.TryAdd(job.Id, job);
                
                _logger.LogInformation("Scheduled job {JobId} of type {JobType}", job.Id, job.JobType);
                return Task.FromResult(job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling job {JobType}", job.JobType);
                throw;
            }
        }

        public Task<bool> CancelJobAsync(string jobId)
        {
            try
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    if (job.Status == JobStatus.Pending || job.Status == JobStatus.Scheduled)
                    {
                        job.Status = JobStatus.Cancelled;
                        _logger.LogInformation("Cancelled job {JobId}", jobId);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling job {JobId}", jobId);
                return Task.FromResult(false);
            }
        }

        public Task<BackgroundJob?> GetJobAsync(string jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return Task.FromResult(job);
        }

        public Task<List<BackgroundJob>> GetJobsAsync(JobStatus? status = null)
        {
            var jobs = _jobs.Values.AsEnumerable();
            
            if (status.HasValue)
            {
                jobs = jobs.Where(j => j.Status == status.Value);
            }
            
            return Task.FromResult(jobs.OrderByDescending(j => j.CreatedAt).ToList());
        }

        public Task<List<BackgroundJob>> GetJobsByTypeAsync(string jobType)
        {
            var jobs = _jobs.Values
                .Where(j => j.JobType.Equals(jobType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
                
            return Task.FromResult(jobs);
        }

        public Task<bool> UpdateJobStatusAsync(string jobId, JobStatus status, string? message = null)
        {
            try
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    job.Status = status;
                    
                    switch (status)
                    {
                        case JobStatus.Running:
                            job.StartedAt = DateTime.UtcNow;
                            break;
                        case JobStatus.Completed:
                        case JobStatus.Failed:
                        case JobStatus.Cancelled:
                            job.CompletedAt = DateTime.UtcNow;
                            break;
                    }
                    
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (status == JobStatus.Failed)
                        {
                            job.ErrorMessage = message;
                        }
                    }
                    
                    _logger.LogInformation("Updated job {JobId} status to {Status}", jobId, status);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId} status", jobId);
                return Task.FromResult(false);
            }
        }

        public Task<List<BackgroundJob>> GetJobsByUserAsync(string userId)
        {
            var jobs = _jobs.Values
                .Where(j => j.CreatedBy == userId || j.AssignedTo == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
                
            return Task.FromResult(jobs);
        }

        public Task<bool> RetryJobAsync(string jobId)
        {
            try
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    if (job.Status == JobStatus.Failed && job.RetryCount < job.MaxRetries)
                    {
                        job.Status = JobStatus.Pending;
                        job.RetryCount++;
                        job.ScheduledAt = DateTime.UtcNow.AddMinutes(5); // Retry after 5 minutes
                        job.ErrorMessage = null;
                        
                        _logger.LogInformation("Retrying job {JobId} (attempt {RetryCount})", jobId, job.RetryCount);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying job {JobId}", jobId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteJobAsync(string jobId)
        {
            try
            {
                if (_jobs.TryRemove(jobId, out var job))
                {
                    _logger.LogInformation("Deleted job {JobId}", jobId);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job {JobId}", jobId);
                return Task.FromResult(false);
            }
        }

        public Task<JobStatistics> GetJobStatisticsAsync()
        {
            var jobs = _jobs.Values.ToList();
            
            var statistics = new JobStatistics
            {
                TotalJobs = jobs.Count,
                PendingJobs = jobs.Count(j => j.Status == JobStatus.Pending),
                RunningJobs = jobs.Count(j => j.Status == JobStatus.Running),
                CompletedJobs = jobs.Count(j => j.Status == JobStatus.Completed),
                FailedJobs = jobs.Count(j => j.Status == JobStatus.Failed),
                CancelledJobs = jobs.Count(j => j.Status == JobStatus.Cancelled),
                JobsByType = jobs.GroupBy(j => j.JobType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                JobsByStatus = jobs.GroupBy(j => j.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
            
            // Calculate average execution time
            var completedJobs = jobs.Where(j => j.Status == JobStatus.Completed && j.StartedAt.HasValue && j.CompletedAt.HasValue);
            if (completedJobs.Any())
            {
                statistics.AverageExecutionTime = completedJobs
                    .Average(j => (j.CompletedAt!.Value - j.StartedAt!.Value).TotalSeconds);
            }
            
            // Calculate success rate
            var totalFinishedJobs = jobs.Count(j => j.Status == JobStatus.Completed || j.Status == JobStatus.Failed);
            if (totalFinishedJobs > 0)
            {
                statistics.SuccessRate = (double)statistics.CompletedJobs / totalFinishedJobs * 100;
            }
            
            return Task.FromResult(statistics);
        }

        public Task<List<BackgroundJob>> GetFailedJobsAsync()
        {
            var failedJobs = _jobs.Values
                .Where(j => j.Status == JobStatus.Failed)
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
                
            return Task.FromResult(failedJobs);
        }

        public Task<bool> PauseJobAsync(string jobId)
        {
            try
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    if (job.Status == JobStatus.Pending || job.Status == JobStatus.Scheduled)
                    {
                        job.Status = JobStatus.Paused;
                        _logger.LogInformation("Paused job {JobId}", jobId);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing job {JobId}", jobId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ResumeJobAsync(string jobId)
        {
            try
            {
                if (_jobs.TryGetValue(jobId, out var job))
                {
                    if (job.Status == JobStatus.Paused)
                    {
                        job.Status = JobStatus.Pending;
                        _logger.LogInformation("Resumed job {JobId}", jobId);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming job {JobId}", jobId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ScheduleRecurringJobAsync(RecurringJob recurringJob)
        {
            try
            {
                recurringJob.Id = Guid.NewGuid().ToString();
                recurringJob.CreatedAt = DateTime.UtcNow;
                
                // Calculate next run time based on cron expression
                recurringJob.NextRunAt = CalculateNextRunTime(recurringJob.CronExpression);
                
                _recurringJobs.TryAdd(recurringJob.Id, recurringJob);
                
                _logger.LogInformation("Scheduled recurring job {JobId} with cron {CronExpression}", 
                    recurringJob.Id, recurringJob.CronExpression);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling recurring job {JobType}", recurringJob.JobType);
                return Task.FromResult(false);
            }
        }

        public Task<bool> CancelRecurringJobAsync(string recurringJobId)
        {
            try
            {
                if (_recurringJobs.TryGetValue(recurringJobId, out var recurringJob))
                {
                    recurringJob.IsEnabled = false;
                    _logger.LogInformation("Cancelled recurring job {JobId}", recurringJobId);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring job {JobId}", recurringJobId);
                return Task.FromResult(false);
            }
        }

        public Task<List<RecurringJob>> GetRecurringJobsAsync()
        {
            var recurringJobs = _recurringJobs.Values
                .OrderByDescending(j => j.CreatedAt)
                .ToList();
                
            return Task.FromResult(recurringJobs);
        }

        private void ProcessJobs(object? state)
        {
            try
            {
                lock (_lockObject)
                {
                    // Process pending jobs
                    var pendingJobs = _jobs.Values
                        .Where(j => j.Status == JobStatus.Pending && 
                                   j.ScheduledAt <= DateTime.UtcNow)
                        .OrderBy(j => j.Priority)
                        .ThenBy(j => j.ScheduledAt)
                        .ToList();

                    foreach (var job in pendingJobs)
                    {
                        _ = Task.Run(() => ExecuteJobAsync(job));
                    }

                    // Process recurring jobs
                    var recurringJobs = _recurringJobs.Values
                        .Where(j => j.IsEnabled && 
                                   j.NextRunAt <= DateTime.UtcNow)
                        .ToList();

                    foreach (var recurringJob in recurringJobs)
                    {
                        _ = Task.Run(() => ExecuteRecurringJobAsync(recurringJob));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing jobs");
            }
        }

        private async Task ExecuteJobAsync(BackgroundJob job)
        {
            try
            {
                await UpdateJobStatusAsync(job.Id, JobStatus.Running);
                
                _logger.LogInformation("Executing job {JobId} of type {JobType}", job.Id, job.JobType);
                
                // Simulate job execution based on job type
                var result = await ExecuteJobByTypeAsync(job);
                
                if (result.Success)
                {
                    job.Results = result.Data;
                    await UpdateJobStatusAsync(job.Id, JobStatus.Completed);
                    _logger.LogInformation("Completed job {JobId}", job.Id);
                }
                else
                {
                    await UpdateJobStatusAsync(job.Id, JobStatus.Failed, result.ErrorMessage);
                    _logger.LogWarning("Failed job {JobId}: {Error}", job.Id, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                await UpdateJobStatusAsync(job.Id, JobStatus.Failed, ex.Message);
                _logger.LogError(ex, "Error executing job {JobId}", job.Id);
            }
        }

        private async Task ExecuteRecurringJobAsync(RecurringJob recurringJob)
        {
            try
            {
                _logger.LogInformation("Executing recurring job {JobId} of type {JobType}", 
                    recurringJob.Id, recurringJob.JobType);
                
                // Create a new background job from the recurring job
                var job = new BackgroundJob
                {
                    JobType = recurringJob.JobType,
                    Name = recurringJob.Name,
                    Description = recurringJob.Description,
                    Parameters = recurringJob.Parameters,
                    CreatedBy = recurringJob.CreatedBy,
                    Tags = recurringJob.Tags,
                    Metadata = recurringJob.Metadata
                };
                
                await ScheduleJobAsync(job);
                
                // Update next run time
                recurringJob.LastRunAt = DateTime.UtcNow;
                recurringJob.NextRunAt = CalculateNextRunTime(recurringJob.CronExpression);
                
                _logger.LogInformation("Scheduled next run for recurring job {JobId} at {NextRun}", 
                    recurringJob.Id, recurringJob.NextRunAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing recurring job {JobId}", recurringJob.Id);
            }
        }

        private async Task<JobExecutionResult> ExecuteJobByTypeAsync(BackgroundJob job)
        {
            // Simulate different job types
            await Task.Delay(1000); // Simulate work
            
            return job.JobType.ToLower() switch
            {
                "system_health_check" => new JobExecutionResult { Success = true, Data = new Dictionary<string, object> { ["health"] = "Good" } },
                "backup_database" => new JobExecutionResult { Success = true, Data = new Dictionary<string, object> { ["backup_size"] = "1.2GB" } },
                "cleanup_logs" => new JobExecutionResult { Success = true, Data = new Dictionary<string, object> { ["files_deleted"] = 150 } },
                "update_software" => new JobExecutionResult { Success = true, Data = new Dictionary<string, object> { ["packages_updated"] = 5 } },
                "security_scan" => new JobExecutionResult { Success = true, Data = new Dictionary<string, object> { ["threats_found"] = 0 } },
                "performance_report" => new JobExecutionResult { Success = true, Data = new Dictionary<string, object> { ["report_generated"] = true } },
                _ => new JobExecutionResult { Success = false, ErrorMessage = "Unknown job type" }
            };
        }

        private DateTime CalculateNextRunTime(string cronExpression)
        {
            // Simplified cron calculation - in a real implementation, you'd use a proper cron library
            // For now, just return a time 1 hour from now
            return DateTime.UtcNow.AddHours(1);
        }

        public void Dispose()
        {
            _jobProcessor?.Dispose();
        }
    }

    public class JobExecutionResult
    {
        public bool Success { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
