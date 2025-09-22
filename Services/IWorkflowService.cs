using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public interface IWorkflowService
    {
        Task<string> CreateWorkflowAsync(WorkflowDefinition workflow);
        Task<bool> ExecuteWorkflowAsync(string workflowId, Dictionary<string, object> inputs);
        Task<WorkflowExecution?> GetWorkflowExecutionAsync(string executionId);
        Task<List<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId);
        Task<List<WorkflowDefinition>> GetWorkflowsAsync();
        Task<WorkflowDefinition?> GetWorkflowAsync(string workflowId);
        Task<bool> UpdateWorkflowAsync(WorkflowDefinition workflow);
        Task<bool> DeleteWorkflowAsync(string workflowId);
        Task<bool> PauseWorkflowExecutionAsync(string executionId);
        Task<bool> ResumeWorkflowExecutionAsync(string executionId);
        Task<bool> CancelWorkflowExecutionAsync(string executionId);
        Task<List<WorkflowExecution>> GetExecutionsByStatusAsync(WorkflowStatus status);
        Task<WorkflowStatistics> GetWorkflowStatisticsAsync();
    }

    public class WorkflowDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public List<WorkflowStep> Steps { get; set; } = new();
        public Dictionary<string, object> Variables { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class WorkflowStep
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Action", "Condition", "Loop", "Parallel"
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public List<string> NextSteps { get; set; } = new();
        public string? Condition { get; set; }
        public int Timeout { get; set; } = 300; // seconds
        public bool IsRequired { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class WorkflowExecution
    {
        public string Id { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? StartedBy { get; set; }
        public Dictionary<string, object> Inputs { get; set; } = new();
        public Dictionary<string, object> Outputs { get; set; } = new();
        public List<StepExecution> StepExecutions { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class StepExecution
    {
        public string Id { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public Dictionary<string, object> Inputs { get; set; } = new();
        public Dictionary<string, object> Outputs { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public enum WorkflowStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        Paused
    }

    public enum StepStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped
    }

    public class WorkflowStatistics
    {
        public int TotalWorkflows { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double SuccessRate { get; set; }
        public double AverageExecutionTime { get; set; }
        public Dictionary<string, int> ExecutionsByStatus { get; set; } = new();
        public Dictionary<string, int> ExecutionsByWorkflow { get; set; } = new();
    }
}
