using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnterpriseITToolkit.Services;
using EnterpriseITToolkit.Models;

namespace EnterpriseITToolkit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MLController : BaseApiController
    {
        private readonly IMachineLearningService _mlService;

        public MLController(
            ILogger<MLController> logger,
            IAuditService auditService,
            IEnhancedAuthenticationService authService,
            IMachineLearningService mlService) 
            : base(logger, auditService, authService)
        {
            _mlService = mlService;
        }

        [HttpPost("predict/system-health")]
        public async Task<IActionResult> PredictSystemHealth([FromBody] SystemHealthData data)
        {
            try
            {
                if (!await HasPermissionAsync("MLPredictions"))
                {
                    return Forbid();
                }

                var prediction = await _mlService.PredictSystemHealthAsync(data);
                
                await LogAuditEventAsync("ML_PREDICTION", "System Health", 
                    $"Predicted system health: {prediction.Prediction}");

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PredictSystemHealth");
            }
        }

        [HttpPost("detect/anomalies")]
        public async Task<IActionResult> DetectAnomalies([FromBody] List<Services.SystemMetric> metrics)
        {
            try
            {
                if (!await HasPermissionAsync("MLPredictions"))
                {
                    return Forbid();
                }

                var anomalies = await _mlService.DetectAnomaliesAsync(metrics);
                
                await LogAuditEventAsync("ML_ANOMALY_DETECTION", "Anomaly Detection", 
                    $"Detected {anomalies.Anomalies.Count} anomalies");

                return Ok(anomalies);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DetectAnomalies");
            }
        }

        [HttpPost("predict/performance")]
        public async Task<IActionResult> PredictPerformance([FromBody] PerformanceData data)
        {
            try
            {
                if (!await HasPermissionAsync("MLPredictions"))
                {
                    return Forbid();
                }

                var prediction = await _mlService.PredictPerformanceAsync(data);
                
                await LogAuditEventAsync("ML_PREDICTION", "Performance", 
                    $"Predicted performance for {data.MetricName}");

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PredictPerformance");
            }
        }

        [HttpPost("predict/threats")]
        public async Task<IActionResult> PredictThreats([FromBody] SecurityEventData data)
        {
            try
            {
                if (!await HasPermissionAsync("MLPredictions"))
                {
                    return Forbid();
                }

                var prediction = await _mlService.PredictThreatsAsync(data);
                
                await LogAuditEventAsync("ML_PREDICTION", "Threat Prediction", 
                    $"Predicted threat: {prediction.ThreatType}");

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PredictThreats");
            }
        }

        [HttpPost("predict/maintenance")]
        public async Task<IActionResult> PredictMaintenance([FromBody] SystemData data)
        {
            try
            {
                if (!await HasPermissionAsync("MLPredictions"))
                {
                    return Forbid();
                }

                var prediction = await _mlService.PredictMaintenanceAsync(data);
                
                await LogAuditEventAsync("ML_PREDICTION", "Maintenance", 
                    $"Predicted maintenance for {data.SystemId}");

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PredictMaintenance");
            }
        }

        [HttpPost("predict/capacity")]
        public async Task<IActionResult> PredictCapacity([FromBody] ResourceData data)
        {
            try
            {
                if (!await HasPermissionAsync("MLPredictions"))
                {
                    return Forbid();
                }

                var prediction = await _mlService.PredictCapacityAsync(data);
                
                await LogAuditEventAsync("ML_PREDICTION", "Capacity", 
                    $"Predicted capacity for {data.ResourceType}");

                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "PredictCapacity");
            }
        }

        [HttpPost("models/{modelType}/train")]
        public async Task<IActionResult> TrainModel(string modelType, [FromBody] List<TrainingData> trainingData)
        {
            try
            {
                if (!await HasPermissionAsync("MLManagement"))
                {
                    return Forbid();
                }

                var success = await _mlService.TrainModelAsync(modelType, trainingData);
                
                await LogAuditEventAsync("ML_MODEL_TRAINING", "Model Training", 
                    $"Trained model {modelType} with {trainingData.Count} samples");

                if (success)
                {
                    return Ok(new { message = "Model trained successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to train model" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "TrainModel");
            }
        }

        [HttpGet("models/{modelType}/performance")]
        public async Task<IActionResult> GetModelPerformance(string modelType)
        {
            try
            {
                if (!await HasPermissionAsync("MLManagement"))
                {
                    return Forbid();
                }

                var performance = await _mlService.GetModelPerformanceAsync(modelType);
                return Ok(performance);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetModelPerformance");
            }
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetAvailableModels()
        {
            try
            {
                if (!await HasPermissionAsync("MLManagement"))
                {
                    return Forbid();
                }

                var models = await _mlService.GetAvailableModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetAvailableModels");
            }
        }

        [HttpPost("models/{modelType}/update")]
        public async Task<IActionResult> UpdateModel(string modelType, [FromBody] List<TrainingData> newData)
        {
            try
            {
                if (!await HasPermissionAsync("MLManagement"))
                {
                    return Forbid();
                }

                var success = await _mlService.UpdateModelAsync(modelType, newData);
                
                await LogAuditEventAsync("ML_MODEL_UPDATE", "Model Update", 
                    $"Updated model {modelType} with {newData.Count} new samples");

                if (success)
                {
                    return Ok(new { message = "Model updated successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to update model" });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, "UpdateModel");
            }
        }

        [HttpPost("insights/{insightType}")]
        public async Task<IActionResult> GetInsights(string insightType, [FromBody] Dictionary<string, object> parameters)
        {
            try
            {
                if (!await HasPermissionAsync("MLInsights"))
                {
                    return Forbid();
                }

                var insights = await _mlService.GetInsightsAsync(insightType, parameters);
                
                await LogAuditEventAsync("ML_INSIGHTS", "ML Insights", 
                    $"Generated insights for type {insightType}");

                return Ok(insights);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetInsights");
            }
        }

        [HttpGet("recommendations/{category}")]
        public async Task<IActionResult> GetRecommendations(string category)
        {
            try
            {
                if (!await HasPermissionAsync("MLInsights"))
                {
                    return Forbid();
                }

                var recommendations = await _mlService.GetRecommendationsAsync(category);
                
                await LogAuditEventAsync("ML_RECOMMENDATIONS", "ML Recommendations", 
                    $"Retrieved recommendations for category {category}");

                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetRecommendations");
            }
        }
    }
}
