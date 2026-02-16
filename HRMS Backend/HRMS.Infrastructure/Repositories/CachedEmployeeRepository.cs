using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Repositories
{
    /// <summary>
    /// Decorator pattern for caching Employee repository operations
    /// Wraps the actual repository and adds caching layer
    /// </summary>
    public class CachedEmployeeRepository : IEmployeeRepository
    {
        private readonly IEmployeeRepository _inner;
        private readonly ICacheService _cache;
        private readonly ILogger<CachedEmployeeRepository> _logger;

        // Cache key constants
        private const string EMPLOYEE_LIST_KEY = "employees:all";
        private const string EMPLOYEE_KEY_PREFIX = "employee:";
        private const string EMPLOYEE_SEARCH_PREFIX = "employee:search:";

        // Cache expiration times
        private static readonly TimeSpan ListCacheExpiration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan SingleCacheExpiration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SearchCacheExpiration = TimeSpan.FromMinutes(2);

        public CachedEmployeeRepository(
            IEmployeeRepository inner,
            ICacheService cache,
            ILogger<CachedEmployeeRepository> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<Employee>> GetPagedAsync(PaginationParams paginationParams)
        {
            // Generate a cache key based on paging params
            var cacheKey = $"employees:paged:{paginationParams.PageNumber}:{paginationParams.PageSize}:{paginationParams.SortBy}:{paginationParams.SortDescending}:{paginationParams.SearchTerm ?? "empty"}";

            // Try get from cache
            var cachedData = await _cache.GetAsync<PagedResult<Employee>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Retrieved paged employees from cache: {CacheKey}", cacheKey);
                return cachedData;
            }

            // Fetch from underlying repository
            var result = await _inner.GetPagedAsync(paginationParams);

            // Cache result
            await _cache.SetAsync(cacheKey, result, ListCacheExpiration);

            _logger.LogInformation("Cache miss - retrieved paged employees from database: {CacheKey}", cacheKey);
            return result;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(bool includeArchived = false)
        {
            var cacheKey = $"{EMPLOYEE_LIST_KEY}:{includeArchived}";

            // Try get from cache
            var cachedData = await _cache.GetAsync<IEnumerable<Employee>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Retrieved employees from cache (includeArchived: {IncludeArchived})", includeArchived);
                return cachedData;
            }

            // Get from database
            _logger.LogInformation("Cache miss - retrieving employees from database");
            var employees = await _inner.GetAllAsync(includeArchived);

            // Cache for 5 minutes
            await _cache.SetAsync(cacheKey, employees, ListCacheExpiration);

            return employees;
        }

        public async Task<Employee?> GetByIdAsync(Guid employeeId)
        {
            var cacheKey = $"{EMPLOYEE_KEY_PREFIX}{employeeId}";

            // Try get from cache
            var cachedData = await _cache.GetAsync<Employee>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Retrieved employee {EmployeeId} from cache", employeeId);
                return cachedData;
            }

            // Get from database
            _logger.LogInformation("Cache miss - retrieving employee {EmployeeId} from database", employeeId);
            var employee = await _inner.GetByIdAsync(employeeId);

            // Cache for 30 minutes if found
            if (employee != null)
            {
                await _cache.SetAsync(cacheKey, employee, SingleCacheExpiration);
            }

            return employee;
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string keyword)
        {
            var cacheKey = $"{EMPLOYEE_SEARCH_PREFIX}{keyword?.ToLower() ?? "empty"}";

            // Try get from cache
            var cachedData = await _cache.GetAsync<IEnumerable<Employee>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Retrieved search results for '{Keyword}' from cache", keyword);
                return cachedData;
            }

            // Search in database
            _logger.LogInformation("Cache miss - searching employees with keyword: '{Keyword}'", keyword);
            var employees = await _inner.SearchAsync(keyword);

            // Cache search results for 2 minutes
            await _cache.SetAsync(cacheKey, employees, SearchCacheExpiration);

            return employees;
        }

        public async Task<Guid> CreateAsync(Employee employee)
        {
            // Create in database
            var employeeId = await _inner.CreateAsync(employee);

            // Invalidate list and search caches
            await InvalidateListCaches();
            await InvalidateSearchCaches();

            _logger.LogInformation("Created employee {EmployeeId} and invalidated list caches", employeeId);

            return employeeId;
        }

        public async Task UpdateAsync(Employee employee)
        {
            // Update in database
            await _inner.UpdateAsync(employee);

            // Invalidate caches
            await _cache.RemoveAsync($"{EMPLOYEE_KEY_PREFIX}{employee.EmployeeId}");
            await InvalidateListCaches();
            await InvalidateSearchCaches();

            _logger.LogInformation("Updated employee {EmployeeId} and invalidated caches", employee.EmployeeId);
        }

        public async Task DeleteAsync(Guid employeeId)
        {
            // Delete from database
            await _inner.DeleteAsync(employeeId);

            // Invalidate caches
            await _cache.RemoveAsync($"{EMPLOYEE_KEY_PREFIX}{employeeId}");
            await InvalidateListCaches();
            await InvalidateSearchCaches();

            _logger.LogInformation("Deleted employee {EmployeeId} and invalidated caches", employeeId);
        }

        public async Task ArchiveAsync(Guid employeeId)
        {
            // Archive in database
            await _inner.ArchiveAsync(employeeId);

            // Invalidate caches (archived status affects both lists)
            await _cache.RemoveAsync($"{EMPLOYEE_KEY_PREFIX}{employeeId}");
            await InvalidateListCaches();
            await InvalidateSearchCaches();

            _logger.LogInformation("Archived employee {EmployeeId} and invalidated caches", employeeId);
        }

        public async Task UnarchiveAsync(Guid employeeId)
        {
            // Unarchive in database
            await _inner.UnarchiveAsync(employeeId);

            // Invalidate caches (archived status affects both lists)
            await _cache.RemoveAsync($"{EMPLOYEE_KEY_PREFIX}{employeeId}");
            await InvalidateListCaches();
            await InvalidateSearchCaches();

            _logger.LogInformation("Unarchived employee {EmployeeId} and invalidated caches", employeeId);
        }

        /// <summary>
        /// Invalidate all employee list caches
        /// </summary>
        private async Task InvalidateListCaches()
        {
            await _cache.RemoveAsync($"{EMPLOYEE_LIST_KEY}:false");
            await _cache.RemoveAsync($"{EMPLOYEE_LIST_KEY}:true");
        }

        /// <summary>
        /// Invalidate all search caches
        /// </summary>
        private async Task InvalidateSearchCaches()
        {
            await _cache.RemoveByPrefixAsync(EMPLOYEE_SEARCH_PREFIX);
        }
    }
}