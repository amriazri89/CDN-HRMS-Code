using HRMS.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Services
{
    /// <summary>
    /// In-Memory cache implementation using IMemoryCache
    /// Fast, local caching - good for single-server applications
    /// </summary>
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InMemoryCacheService> _logger;
        private readonly HashSet<string> _keys; // Track all cache keys for prefix removal

        public InMemoryCacheService(
            IMemoryCache cache,
            ILogger<InMemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _keys = new HashSet<string>();
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_cache.TryGetValue(key, out T? value))
                {
                    _logger.LogDebug("Cache HIT: {Key}", key);
                    return Task.FromResult(value);
                }

                _logger.LogDebug("Cache MISS: {Key}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration,
                    SlidingExpiration = null // Set if you want sliding expiration
                };

                _cache.Set(key, value, cacheOptions);
                _keys.Add(key); // Track the key

                _logger.LogDebug("Cache SET: {Key}, Expiration: {Expiration}", key, expiration);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _cache.Remove(key);
                _keys.Remove(key);

                _logger.LogDebug("Cache REMOVE: {Key}", key);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _keys.Remove(key);
                }

                _logger.LogDebug("Cache REMOVE BY PREFIX: {Prefix}, Removed: {Count}", prefix, keysToRemove.Count);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
                return Task.CompletedTask;
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _cache.TryGetValue(key, out _);
                return Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache key exists: {Key}", key);
                return Task.FromResult(false);
            }
        }
    }
}