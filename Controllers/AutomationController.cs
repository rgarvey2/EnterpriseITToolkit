using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutomationController : BaseApiController
    {
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IWorkflowService _workflowService;

        public AutomationController(
            ILogger<AutomationController> logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService,
            IBackgroundJobService backgroundJobService,
            IWorkflowService workflowService) 
            : base(logger, auditService, authService)
        {
            _backgroundJobService = backgroundJobService;
            _workflowService = workflowService;
        }

        #region Background Jobs

        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var job = new BackgroundJob
                {
                    JobType = request.JobType,
                    Name = request.Name,
                    Description = request.Description,
                    Priority = request.Priority,
                    Parameters = request.Parameters,
                    CreatedBy = GetCurrentUsername(),
                    Tags = request.Tags,
                    MaxRetries = request.MaxRetries,
                    Timeout = request.Timeout
                };

                var jobId = await _backgroundJobService.ScheduleJobAsync(job);
                
                await LogAuditEventAsync("JOB_CREATED", "Background Job", 
                    $"Created job {jobId} of type {request.JobType}");

                return Ok(new { jobId, message = "Job created successfully" });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateJob");
            }
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs([FromQuery] string? status = null)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                JobStatus? jobStatus = null;
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<JobStatus>(status, true, out var parsedStatus))
                {
                    jobStatus = parsedStatus;
                }

                var jobs = await _backgroundJobService.GetJobsAsync(jobStatus);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetJobs");
            }
        }

        [HttpGet("jobs/{jobId}")]
        public async Task<IActionResult> GetJob(string jobId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var job = await _backgroundJobService.GetJobAsync(jobId);
                if (job == null)
                {
                    return NotFound(new { error = "Job not found" });
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetJob");
            }
        }

        [HttpPost("jobs/{jobId}/cancel")]
        public async Task<IActionResult> CancelJob(string jobId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var success = await _backgroundJobService.CancelJobAsync(jobId);
                
                await LogAuditEventAsync("JOB_CANCELLED", "Background Job", 
                    $"Cancelled job {jobId}");

                if (success)
                {
                    return Ok(new { message = "Job cancelled successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to cancel job" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CancelJob");
            }
        }

        [HttpPost("jobs/{jobId}/retry")]
        public async Task<IActionResult> RetryJob(string jobId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var success = await _backgroundJobService.RetryJobAsync(jobId);
                
                await LogAuditEventAsync("JOB_RETRIED", "Background Job", 
                    $"Retried job {jobId}");

                if (success)
                {
                    return Ok(new { message = "Job retry scheduled successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to retry job" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "RetryJob");
            }
        }

        [HttpPost("jobs/{jobId}/pause")]
        public async Task<IActionResult> PauseJob(string jobId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var success = await _backgroundJobService.PauseJobAsync(jobId);
                
                await LogAuditEventAsync("JOB_PAUSED", "Background Job", 
                    $"Paused job {jobId}");

                if (success)
                {
                    return Ok(new { message = "Job paused successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to pause job" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PauseJob");
            }
        }

        [HttpPost("jobs/{jobId}/resume")]
        public async Task<IActionResult> ResumeJob(string jobId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var success = await _backgroundJobService.ResumeJobAsync(jobId);
                
                await LogAuditEventAsync("JOB_RESUMED", "Background Job", 
                    $"Resumed job {jobId}");

                if (success)
                {
                    return Ok(new { message = "Job resumed successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to resume job" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ResumeJob");
            }
        }

        [HttpGet("jobs/statistics")]
        public async Task<IActionResult> GetJobStatistics()
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var statistics = await _backgroundJobService.GetJobStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetJobStatistics");
            }
        }

        #endregion

        #region Workflows

        [HttpPost("workflows")]
        public async Task<IActionResult> CreateWorkflow([FromBody] WorkflowDefinition workflow)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                workflow.CreatedBy = GetCurrentUsername();
                var workflowId = await _workflowService.CreateWorkflowAsync(workflow);
                
                await LogAuditEventAsync("WORKFLOW_CREATED", "Workflow", 
                    $"Created workflow {workflowId}: {workflow.Name}");

                return Ok(new { workflowId, message = "Workflow created successfully" });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateWorkflow");
            }
        }

        [HttpGet("workflows")]
        public async Task<IActionResult> GetWorkflows()
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var workflows = await _workflowService.GetWorkflowsAsync();
                return Ok(workflows);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetWorkflows");
            }
        }

        [HttpGet("workflows/{workflowId}")]
        public async Task<IActionResult> GetWorkflow(string workflowId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var workflow = await _workflowService.GetWorkflowAsync(workflowId);
                if (workflow == null)
                {
                    return NotFound(new { error = "Workflow not found" });
                }

                return Ok(workflow);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetWorkflow");
            }
        }

        [HttpPost("workflows/{workflowId}/execute")]
        public async Task<IActionResult> ExecuteWorkflow(string workflowId, [FromBody] ExecuteWorkflowRequest request)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var success = await _workflowService.ExecuteWorkflowAsync(workflowId, request.Inputs);
                
                await LogAuditEventAsync("WORKFLOW_EXECUTED", "Workflow", 
                    $"Executed workflow {workflowId}");

                if (success)
                {
                    return Ok(new { message = "Workflow execution started successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to start workflow execution" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ExecuteWorkflow");
            }
        }

        [HttpGet("workflows/{workflowId}/executions")]
        public async Task<IActionResult> GetWorkflowExecutions(string workflowId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var executions = await _workflowService.GetWorkflowExecutionsAsync(workflowId);
                return Ok(executions);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetWorkflowExecutions");
            }
        }

        [HttpGet("workflows/executions/{executionId}")]
        public async Task<IActionResult> GetWorkflowExecution(string executionId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var execution = await _workflowService.GetWorkflowExecutionAsync(executionId);
                if (execution == null)
                {
                    return NotFound(new { error = "Workflow execution not found" });
                }

                return Ok(execution);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetWorkflowExecution");
            }
        }

        [HttpPost("workflows/executions/{executionId}/cancel")]
        public async Task<IActionResult> CancelWorkflowExecution(string executionId)
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var success = await _workflowService.CancelWorkflowExecutionAsync(executionId);
                
                await LogAuditEventAsync("WORKFLOW_CANCELLED", "Workflow", 
                    $"Cancelled workflow execution {executionId}");

                if (success)
                {
                    return Ok(new { message = "Workflow execution cancelled successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to cancel workflow execution" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CancelWorkflowExecution");
            }
        }

        [HttpGet("workflows/statistics")]
        public async Task<IActionResult> GetWorkflowStatistics()
        {
            try
            {
                if (!await HasPermissionAsync("AutomationManagement"))
                {
                    return Forbid();
                }

                var statistics = await _workflowService.GetWorkflowStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetWorkflowStatistics");
            }
        }

        #endregion
    }

    public class CreateJobRequest
    {
        public string JobType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public JobPriority Priority { get; set; } = JobPriority.Normal;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int MaxRetries { get; set; } = 3;
        public TimeSpan? Timeout { get; set; }
    }

    public class ExecuteWorkflowRequest
    {
        public Dictionary<string, object> Inputs { get; set; } = new();
    }
}
