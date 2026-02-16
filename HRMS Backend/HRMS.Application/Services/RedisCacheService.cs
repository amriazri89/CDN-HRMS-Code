using HRMS.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace HRMS.Infrastructure.Services
{
	/// <summary>
	/// Redis distributed cache implementation
	/// Supports multi-server applications and larger datasets
	/// </summary>
	public class RedisCacheService : ICacheService
	{
		private readonly IDistributedCache _cache;
		private readonly IConnectionMultiplexer _redis;
		private readonly ILogger<RedisCacheService> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public RedisCacheService(
			IDistributedCache cache,
			IConnectionMultiplexer redis,
			ILogger<RedisCacheService> logger)
		{
			_cache = cache;
			_redis = redis;
			_logger = logger;
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				WriteIndented = false
			};
		}

		public async Task<T?> GetAsync<T>(string key) where T : class
		{
			try
			{
				var cachedData = await _cache.GetStringAsync(key);

				if (cachedData == null)
				{
					_logger.LogDebug("Redis Cache MISS: {Key}", key);
					return null;
				}

				_logger.LogDebug("Redis Cache HIT: {Key}", key);
				return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting Redis cache key: {Key}", key);
				return null;
			}
		}

		public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
		{
			try
			{
				var serializedData = JsonSerializer.Serialize(value, _jsonOptions);

				var options = new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = expiration
				};

				await _cache.SetStringAsync(key, serializedData, options);

				_logger.LogDebug("Redis Cache SET: {Key}, Expiration: {Expiration}", key, expiration);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error setting Redis cache key: {Key}", key);
			}
		}

		public async Task RemoveAsync(string key)
		{
			try
			{
				await _cache.RemoveAsync(key);
				_logger.LogDebug("Redis Cache REMOVE: {Key}", key);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error removing Redis cache key: {Key}", key);
			}
		}

		public async Task RemoveByPrefixAsync(string prefix)
		{
			try
			{
				var server = _redis.GetServer(_redis.GetEndPoints()[0]);
				var keys = server.Keys(pattern: $"{prefix}*");

				var db = _redis.GetDatabase();
				var tasks = new List<Task>();

				foreach (var key in keys)
				{
					tasks.Add(db.KeyDeleteAsync(key));
				}

				await Task.WhenAll(tasks);

				_logger.LogDebug("Redis Cache REMOVE BY PREFIX: {Prefix}", prefix);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error removing Redis cache by prefix: {Prefix}", prefix);
			}
		}

		public async Task<bool> ExistsAsync(string key)
		{
			try
			{
				var db = _redis.GetDatabase();
				return await db.KeyExistsAsync(key);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking Redis cache key exists: {Key}", key);
				return false;
			}
		}
	}
}