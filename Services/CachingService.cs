using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;

namespace EnterpriseITToolkit.Services
{
    public interface ICachingService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
        Task<long> GetExpirationAsync(string key);
        Task SetExpirationAsync(string key, TimeSpan expiration);
        Task<Dictionary<string, T>> GetByPatternAsync<T>(string pattern);
        Task ClearAllAsync();
        Task<long> GetMemoryUsageAsync();
        Task<Dictionary<string, object>> GetCacheStatsAsync();
    }

    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDatabase? _redisDatabase;
        private readonly ILogger<CachingService> _logger;
        private readonly bool _useRedis;

        public CachingService(
            IMemoryCache memoryCache,
            IConnectionMultiplexer? redisConnection,
            ILogger<CachingService> logger,
            IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _useRedis = redisConnection != null && !string.IsNullOrEmpty(configuration.GetConnectionString("Redis"));
            
            if (_useRedis)
            {
                _redisDatabase = redisConnection!.GetDatabase();
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    var value = await _redisDatabase.StringGetAsync(key);
                    if (value.HasValue)
                    {
                        return JsonSerializer.Deserialize<T>(value!);
                    }
                }
                else
                {
                    if (_memoryCache.TryGetValue(key, out T? cachedValue))
                    {
                        return cachedValue;
                    }
                }

                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return default(T);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    var serializedValue = JsonSerializer.Serialize(value);
                    await _redisDatabase.StringSetAsync(key, serializedValue, expiration);
                }
                else
                {
                    var options = new MemoryCacheEntryOptions();
                    if (expiration.HasValue)
                    {
                        options.SetAbsoluteExpiration(expiration.Value);
                    }
                    else
                    {
                        options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default 30 minutes
                    }

                    _memoryCache.Set(key, value, options);
                }

                _logger.LogDebug("Cached value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    await _redisDatabase.KeyDeleteAsync(key);
                }
                else
                {
                    _memoryCache.Remove(key);
                }

                _logger.LogDebug("Removed cache value for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
                    var keys = server.Keys(pattern: pattern);
                    await _redisDatabase.KeyDeleteAsync(keys.ToArray());
                }
                else
                {
                    // For memory cache, we can't easily remove by pattern
                    // This would require maintaining a separate index
                    _logger.LogWarning("RemoveByPatternAsync not fully supported for memory cache");
                }

                _logger.LogDebug("Removed cache values for pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    return await _redisDatabase.KeyExistsAsync(key);
                }
                else
                {
                    return _memoryCache.TryGetValue(key, out _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<long> GetExpirationAsync(string key)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    var ttl = await _redisDatabase.KeyTimeToLiveAsync(key);
                return (long)(ttl?.TotalSeconds ?? -1);
                }
                else
                {
                    // Memory cache doesn't expose TTL directly
                    return -1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache expiration for key: {Key}", key);
                return -1;
            }
        }

        public async Task SetExpirationAsync(string key, TimeSpan expiration)
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    await _redisDatabase.KeyExpireAsync(key, expiration);
                }
                else
                {
                    // For memory cache, we'd need to re-set the value with new expiration
                    _logger.LogWarning("SetExpirationAsync not fully supported for memory cache");
                }

                _logger.LogDebug("Set expiration for key: {Key} to {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache expiration for key: {Key}", key);
            }
        }

        public async Task<Dictionary<string, T>> GetByPatternAsync<T>(string pattern)
        {
            try
            {
                var result = new Dictionary<string, T>();

                if (_useRedis && _redisDatabase != null)
                {
                    var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
                    var keys = server.Keys(pattern: pattern);

                    foreach (var key in keys)
                    {
                        var value = await _redisDatabase.StringGetAsync(key);
                        if (value.HasValue)
                        {
                            var deserializedValue = JsonSerializer.Deserialize<T>(value!);
                            if (deserializedValue != null)
                            {
                                result[key.ToString()] = deserializedValue;
                            }
                        }
                    }
                }
                else
                {
                    // For memory cache, we can't easily get by pattern
                    _logger.LogWarning("GetByPatternAsync not fully supported for memory cache");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache values for pattern: {Pattern}", pattern);
                return new Dictionary<string, T>();
            }
        }

        public async Task ClearAllAsync()
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
                    await server.FlushDatabaseAsync();
                }
                else
                {
                    // For memory cache, we can't easily clear all
                    _logger.LogWarning("ClearAllAsync not fully supported for memory cache");
                }

                _logger.LogInformation("Cleared all cache values");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache values");
            }
        }

        public Task<long> GetMemoryUsageAsync()
        {
            try
            {
                if (_useRedis && _redisDatabase != null)
                {
                    // Simplified Redis memory usage - would need proper implementation
                    return Task.FromResult(0L);
                }
                else
                {
                    // For memory cache, we can't easily get memory usage
                    return Task.FromResult(GC.GetTotalMemory(false));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache memory usage");
                return Task.FromResult(0L);
            }
        }

        public Task<Dictionary<string, object>> GetCacheStatsAsync()
        {
            try
            {
                var stats = new Dictionary<string, object>();

                if (_useRedis && _redisDatabase != null)
                {
                    stats["CacheType"] = "Redis";
                    stats["Connected"] = _redisDatabase.Multiplexer.IsConnected;
                    // Note: Redis info parsing simplified for now
                }
                else
                {
                    stats["CacheType"] = "Memory";
                    stats["TotalMemory"] = GC.GetTotalMemory(false);
                    stats["Gen0Collections"] = GC.CollectionCount(0);
                    stats["Gen1Collections"] = GC.CollectionCount(1);
                    stats["Gen2Collections"] = GC.CollectionCount(2);
                }

                return Task.FromResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return Task.FromResult(new Dictionary<string, object> { { "Error", ex.Message } });
            }
        }
    }
}
