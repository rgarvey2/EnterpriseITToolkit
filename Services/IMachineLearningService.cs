using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Services
{
    public interface IMachineLearningService
    {
        Task<PredictionResult> PredictSystemHealthAsync(SystemHealthData data);
        Task<AnomalyDetectionResult> DetectAnomaliesAsync(List<SystemMetric> metrics);
        Task<PerformancePrediction> PredictPerformanceAsync(PerformanceData data);
        Task<ThreatPrediction> PredictThreatsAsync(SecurityEventData data);
        Task<MaintenancePrediction> PredictMaintenanceAsync(SystemData data);
        Task<CapacityPrediction> PredictCapacityAsync(ResourceData data);
        Task<bool> TrainModelAsync(string modelType, List<TrainingData> trainingData);
        Task<ModelPerformance> GetModelPerformanceAsync(string modelType);
        Task<List<MLModel>> GetAvailableModelsAsync();
        Task<bool> UpdateModelAsync(string modelType, List<TrainingData> newData);
        Task<MLInsights> GetInsightsAsync(string insightType, Dictionary<string, object> parameters);
        Task<List<Recommendation>> GetRecommendationsAsync(string category);
    }

    public class PredictionResult
    {
        public string ModelType { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Prediction { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
        public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
        public List<string> Factors { get; set; } = new();
        public double Accuracy { get; set; }
    }

    public class AnomalyDetectionResult
    {
        public List<Anomaly> Anomalies { get; set; } = new();
        public double AnomalyScore { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Context { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class Anomaly
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Score { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public string Severity { get; set; } = string.Empty;
    }

    public class PerformancePrediction
    {
        public string Metric { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double PredictedValue { get; set; }
        public DateTime PredictionTime { get; set; }
        public double Confidence { get; set; }
        public string Trend { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
        public List<DataPoint> HistoricalData { get; set; } = new();
        public List<DataPoint> PredictedData { get; set; } = new();
    }

    public class ThreatPrediction
    {
        public string ThreatType { get; set; } = string.Empty;
        public double Probability { get; set; }
        public DateTime PredictedTime { get; set; }
        public string Severity { get; set; } = string.Empty;
        public List<string> Indicators { get; set; } = new();
        public Dictionary<string, object> RiskFactors { get; set; } = new();
        public List<string> MitigationStrategies { get; set; } = new();
    }

    public class MaintenancePrediction
    {
        public string Component { get; set; } = string.Empty;
        public DateTime PredictedFailureDate { get; set; }
        public double FailureProbability { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public int EstimatedDowntime { get; set; } // minutes
        public double CostEstimate { get; set; }
        public List<string> RecommendedActions { get; set; } = new();
        public string Priority { get; set; } = string.Empty;
    }

    public class CapacityPrediction
    {
        public string Resource { get; set; } = string.Empty;
        public double CurrentUsage { get; set; }
        public double PredictedUsage { get; set; }
        public DateTime PredictedTime { get; set; }
        public double UtilizationThreshold { get; set; }
        public bool IsOverCapacity { get; set; }
        public List<DataPoint> UsageHistory { get; set; } = new();
        public List<DataPoint> PredictedUsageData { get; set; } = new();
        public List<string> ScalingRecommendations { get; set; } = new();
    }

    public class SystemHealthData
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double NetworkUsage { get; set; }
        public int ProcessCount { get; set; }
        public int ServiceCount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    }

    public class SystemMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class PerformanceData
    {
        public string MetricName { get; set; } = string.Empty;
        public List<DataPoint> HistoricalData { get; set; } = new();
        public Dictionary<string, object> Context { get; set; } = new();
        public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
    }

    public class SecurityEventData
    {
        public List<SecurityEvent> Events { get; set; } = new();
        public Dictionary<string, object> SystemContext { get; set; } = new();
        public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
    }

    public class SystemData
    {
        public string SystemId { get; set; } = string.Empty;
        public Dictionary<string, object> Metrics { get; set; } = new();
        public List<MaintenanceHistory> MaintenanceHistory { get; set; } = new();
        public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
    }

    public class ResourceData
    {
        public string ResourceType { get; set; } = string.Empty;
        public List<DataPoint> UsageHistory { get; set; } = new();
        public Dictionary<string, object> Configuration { get; set; } = new();
        public DateTime AnalysisTime { get; set; } = DateTime.UtcNow;
    }

    public class TrainingData
    {
        public Dictionary<string, object> Features { get; set; } = new();
        public object Label { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ModelPerformance
    {
        public string ModelType { get; set; } = string.Empty;
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public DateTime LastTrained { get; set; }
        public int TrainingSamples { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    public class MLModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastTrained { get; set; }
        public double Accuracy { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public List<string> Features { get; set; } = new();
    }

    public class MLInsights
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> KeyFindings { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public double Confidence { get; set; }
    }

    public class Recommendation
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double Impact { get; set; }
        public double Effort { get; set; }
        public List<string> Steps { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class MaintenanceHistory
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Cost { get; set; }
        public int Downtime { get; set; } // minutes
    }
}
