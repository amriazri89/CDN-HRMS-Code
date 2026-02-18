using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.SearchEmployees
{
    /// <summary>
    /// Handler that executes SearchEmployeesQuery
    /// Uses Dapper repository with LIKE query
    /// </summary>
    public class SearchEmployeesQueryHandler : IRequestHandler<SearchEmployeesQuery, IEnumerable<Employee>>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<SearchEmployeesQueryHandler> _logger;

        public SearchEmployeesQueryHandler(
            IEmployeeRepository repository,
            ILogger<SearchEmployeesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Employee>> Handle(SearchEmployeesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching employees: {Keyword}", request.Keyword);

            if (string.IsNullOrWhiteSpace(request.Keyword))
            {
                _logger.LogWarning("Empty search keyword provided");
                return Enumerable.Empty<Employee>();
            }

            // Call Dapper repository (executes SQL with LIKE)
            // Your existing SearchAsync includes:
            // - WHERE EmployeeNumber LIKE @Keyword OR Name LIKE @Keyword
            // - AND IsArchived = 0
            var results = await _repository.SearchAsync(request.Keyword);

            _logger.LogInformation("Found {Count} employees matching '{Keyword}'",
                results.Count(), request.Keyword);

            return results;
        }
    }
}