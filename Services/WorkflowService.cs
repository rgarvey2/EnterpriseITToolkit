using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace EnterpriseITToolkit.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly ILogger<WorkflowService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly ConcurrentDictionary<string, WorkflowDefinition> _workflows;
        private readonly ConcurrentDictionary<string, WorkflowExecution> _executions;

        public WorkflowService(
            ILogger<WorkflowService> logger, 
            IConfiguration configuration,
            IBackgroundJobService backgroundJobService)
        {
            _logger = logger;
            _configuration = configuration;
            _backgroundJobService = backgroundJobService;
            _workflows = new ConcurrentDictionary<string, WorkflowDefinition>();
            _executions = new ConcurrentDictionary<string, WorkflowExecution>();
        }

        public Task<string> CreateWorkflowAsync(WorkflowDefinition workflow)
        {
            try
            {
                workflow.Id = Guid.NewGuid().ToString();
                workflow.CreatedAt = DateTime.UtcNow;
                
                _workflows.TryAdd(workflow.Id, workflow);
                
                _logger.LogInformation("Created workflow {WorkflowId}: {WorkflowName}", workflow.Id, workflow.Name);
                return Task.FromResult(workflow.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow {WorkflowName}", workflow.Name);
                throw;
            }
        }

        public async Task<bool> ExecuteWorkflowAsync(string workflowId, Dictionary<string, object> inputs)
        {
            try
            {
                if (!_workflows.TryGetValue(workflowId, out var workflow))
                {
                    _logger.LogWarning("Workflow {WorkflowId} not found", workflowId);
                    return false;
                }

                var execution = new WorkflowExecution
                {
                    Id = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId,
                    WorkflowName = workflow.Name,
                    Status = WorkflowStatus.Pending,
                    StartedAt = DateTime.UtcNow,
                    Inputs = inputs,
                    Variables = new Dictionary<string, object>(workflow.Variables)
                };

                _executions.TryAdd(execution.Id, execution);
                
                // Start workflow execution as a background job
                var job = new BackgroundJob
                {
                    JobType = "workflow_execution",
                    Name = $"Execute Workflow: {workflow.Name}",
                    Description = $"Execute workflow {workflow.Name}",
                    Parameters = new Dictionary<string, object>
                    {
                        ["workflowId"] = workflowId,
                        ["executionId"] = execution.Id,
                        ["inputs"] = inputs
                    },
                    Priority = JobPriority.Normal
                };

                await _backgroundJobService.ScheduleJobAsync(job);
                
                _logger.LogInformation("Started workflow execution {ExecutionId} for workflow {WorkflowId}", 
                    execution.Id, workflowId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing workflow {WorkflowId}", workflowId);
                return false;
            }
        }

        public Task<WorkflowExecution?> GetWorkflowExecutionAsync(string executionId)
        {
            _executions.TryGetValue(executionId, out var execution);
            return Task.FromResult(execution);
        }

        public Task<List<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId)
        {
            var executions = _executions.Values
                .Where(e => e.WorkflowId == workflowId)
                .OrderByDescending(e => e.StartedAt)
                .ToList();
                
            return Task.FromResult(executions);
        }

        public Task<List<WorkflowDefinition>> GetWorkflowsAsync()
        {
            var workflows = _workflows.Values
                .OrderByDescending(w => w.CreatedAt)
                .ToList();
                
            return Task.FromResult(workflows);
        }

        public Task<WorkflowDefinition?> GetWorkflowAsync(string workflowId)
        {
            _workflows.TryGetValue(workflowId, out var workflow);
            return Task.FromResult(workflow);
        }

        public Task<bool> UpdateWorkflowAsync(WorkflowDefinition workflow)
        {
            try
            {
                if (_workflows.TryGetValue(workflow.Id, out var existingWorkflow))
                {
                    workflow.UpdatedAt = DateTime.UtcNow;
                    _workflows.TryUpdate(workflow.Id, workflow, existingWorkflow);
                    
                    _logger.LogInformation("Updated workflow {WorkflowId}", workflow.Id);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow {WorkflowId}", workflow.Id);
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteWorkflowAsync(string workflowId)
        {
            try
            {
                if (_workflows.TryRemove(workflowId, out var workflow))
                {
                    _logger.LogInformation("Deleted workflow {WorkflowId}: {WorkflowName}", workflowId, workflow.Name);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", workflowId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> PauseWorkflowExecutionAsync(string executionId)
        {
            try
            {
                if (_executions.TryGetValue(executionId, out var execution))
                {
                    if (execution.Status == WorkflowStatus.Running)
                    {
                        execution.Status = WorkflowStatus.Paused;
                        _logger.LogInformation("Paused workflow execution {ExecutionId}", executionId);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing workflow execution {ExecutionId}", executionId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> ResumeWorkflowExecutionAsync(string executionId)
        {
            try
            {
                if (_executions.TryGetValue(executionId, out var execution))
                {
                    if (execution.Status == WorkflowStatus.Paused)
                    {
                        execution.Status = WorkflowStatus.Running;
                        _logger.LogInformation("Resumed workflow execution {ExecutionId}", executionId);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming workflow execution {ExecutionId}", executionId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> CancelWorkflowExecutionAsync(string executionId)
        {
            try
            {
                if (_executions.TryGetValue(executionId, out var execution))
                {
                    if (execution.Status == WorkflowStatus.Running || execution.Status == WorkflowStatus.Paused)
                    {
                        execution.Status = WorkflowStatus.Cancelled;
                        execution.CompletedAt = DateTime.UtcNow;
                        _logger.LogInformation("Cancelled workflow execution {ExecutionId}", executionId);
                        return Task.FromResult(true);
                    }
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling workflow execution {ExecutionId}", executionId);
                return Task.FromResult(false);
            }
        }

        public Task<List<WorkflowExecution>> GetExecutionsByStatusAsync(WorkflowStatus status)
        {
            var executions = _executions.Values
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.StartedAt)
                .ToList();
                
            return Task.FromResult(executions);
        }

        public Task<WorkflowStatistics> GetWorkflowStatisticsAsync()
        {
            var executions = _executions.Values.ToList();
            
            var statistics = new WorkflowStatistics
            {
                TotalWorkflows = _workflows.Count,
                TotalExecutions = executions.Count,
                SuccessfulExecutions = executions.Count(e => e.Status == WorkflowStatus.Completed),
                FailedExecutions = executions.Count(e => e.Status == WorkflowStatus.Failed),
                ExecutionsByStatus = executions.GroupBy(e => e.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                ExecutionsByWorkflow = executions.GroupBy(e => e.WorkflowName)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
            
            // Calculate success rate
            var totalFinishedExecutions = executions.Count(e => 
                e.Status == WorkflowStatus.Completed || e.Status == WorkflowStatus.Failed);
            if (totalFinishedExecutions > 0)
            {
                statistics.SuccessRate = (double)statistics.SuccessfulExecutions / totalFinishedExecutions * 100;
            }
            
            // Calculate average execution time
            var completedExecutions = executions.Where(e => 
                e.Status == WorkflowStatus.Completed && e.CompletedAt.HasValue);
            if (completedExecutions.Any())
            {
                statistics.AverageExecutionTime = completedExecutions
                    .Average(e => (e.CompletedAt!.Value - e.StartedAt).TotalSeconds);
            }
            
            return Task.FromResult(statistics);
        }
    }
}
