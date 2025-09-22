using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EnterpriseITToolkit.Services
{
    public interface ICorrelationService
    {
        string GenerateCorrelationId();
        void SetCorrelationId(string correlationId);
        string GetCurrentCorrelationId();
        IDisposable CreateCorrelationScope(string operation);
        void LogWithCorrelation(LogLevel level, string message, params object[] args);
        void LogWithCorrelation(LogLevel level, Exception exception, string message, params object[] args);
    }

    public class CorrelationService : ICorrelationService
    {
        private readonly ILogger<CorrelationService> _logger;
        private static readonly AsyncLocal<string> _correlationId = new();

        public CorrelationService(ILogger<CorrelationService> logger)
        {
            _logger = logger;
        }

        public string GenerateCorrelationId()
        {
            var correlationId = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            _logger.LogDebug("Generated correlation ID: {CorrelationId}", correlationId);
            return correlationId;
        }

        public void SetCorrelationId(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = GenerateCorrelationId();
            }
            
            _correlationId.Value = correlationId;
            _logger.LogDebug("Set correlation ID: {CorrelationId}", correlationId);
        }

        public string GetCurrentCorrelationId()
        {
            return _correlationId.Value ?? GenerateCorrelationId();
        }

        public IDisposable CreateCorrelationScope(string operation)
        {
            var correlationId = GetCurrentCorrelationId();
            return new CorrelationScope(correlationId, operation, _logger);
        }

        public void LogWithCorrelation(LogLevel level, string message, params object[] args)
        {
            var correlationId = GetCurrentCorrelationId();
            var enrichedMessage = $"[{correlationId}] {message}";
            
            switch (level)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(enrichedMessage, args);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(enrichedMessage, args);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(enrichedMessage, args);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(enrichedMessage, args);
                    break;
                case LogLevel.Error:
                    _logger.LogError(enrichedMessage, args);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(enrichedMessage, args);
                    break;
            }
        }

        public void LogWithCorrelation(LogLevel level, Exception exception, string message, params object[] args)
        {
            var correlationId = GetCurrentCorrelationId();
            var enrichedMessage = $"[{correlationId}] {message}";
            
            switch (level)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(exception, enrichedMessage, args);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(exception, enrichedMessage, args);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(exception, enrichedMessage, args);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(exception, enrichedMessage, args);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, enrichedMessage, args);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(exception, enrichedMessage, args);
                    break;
            }
        }
    }

    public class CorrelationScope : IDisposable
    {
        private readonly string _correlationId;
        private readonly string _operation;
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;
        private bool _disposed = false;

        public CorrelationScope(string correlationId, string operation, ILogger logger)
        {
            _correlationId = correlationId;
            _operation = operation;
            _logger = logger;
            _stopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation("[{CorrelationId}] Starting operation: {Operation}", 
                _correlationId, _operation);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                _logger.LogInformation("[{CorrelationId}] Completed operation: {Operation} in {Duration}ms", 
                    _correlationId, _operation, _stopwatch.ElapsedMilliseconds);
                _disposed = true;
            }
        }
    }

    public static class CorrelationExtensions
    {
        public static void LogWithCorrelation(this ILogger logger, string correlationId, LogLevel level, 
            string message, params object[] args)
        {
            var enrichedMessage = $"[{correlationId}] {message}";
            
            switch (level)
            {
                case LogLevel.Trace:
                    logger.LogTrace(enrichedMessage, args);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(enrichedMessage, args);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(enrichedMessage, args);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(enrichedMessage, args);
                    break;
                case LogLevel.Error:
                    logger.LogError(enrichedMessage, args);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(enrichedMessage, args);
                    break;
            }
        }

        public static void LogWithCorrelation(this ILogger logger, string correlationId, LogLevel level, 
            Exception exception, string message, params object[] args)
        {
            var enrichedMessage = $"[{correlationId}] {message}";
            
            switch (level)
            {
                case LogLevel.Trace:
                    logger.LogTrace(exception, enrichedMessage, args);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(exception, enrichedMessage, args);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(exception, enrichedMessage, args);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(exception, enrichedMessage, args);
                    break;
                case LogLevel.Error:
                    logger.LogError(exception, enrichedMessage, args);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(exception, enrichedMessage, args);
                    break;
            }
        }
    }
}
