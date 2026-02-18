using System;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces
{
    /// <summary>
    /// Interface for caching service
    /// Supports both in-memory and distributed caching (Redis)
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get cached value by key
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Set cache value with expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;

        /// <summary>
        /// Remove cached value by key
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Remove all cache keys with prefix
        /// </summary>
        Task RemoveByPrefixAsync(string prefix);

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);
    }
}