namespace EnterpriseITToolkit.Services
{
    public interface ITroubleshootingService
    {
        Task<RepairResult> RunStartupRepairAsync();
        Task<RepairResult> RunSystemFileCheckAsync();
        Task<RepairResult> RunDismRepairAsync();
        Task<List<SystemIssue>> DiagnoseSystemIssuesAsync();
    }

    public class RepairResult
    {
        public bool Success { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
    }

    public class SystemIssue
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }
}
