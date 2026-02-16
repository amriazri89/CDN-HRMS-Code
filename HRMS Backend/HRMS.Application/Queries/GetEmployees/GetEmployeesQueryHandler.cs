using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.GetEmployees
{
    /// <summary>
    /// Handler that executes GetEmployeesQuery
    /// Uses Dapper repository (SELECT * FROM Employees)
    /// </summary>
    public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, IEnumerable<Employee>>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<GetEmployeesQueryHandler> _logger;

        public GetEmployeesQueryHandler(
            IEmployeeRepository repository,
            ILogger<GetEmployeesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Employee>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving employees (IncludeArchived: {IncludeArchived})", request.IncludeArchived);

            // Call Dapper repository (executes SELECT query)
            var employees = await _repository.GetAllAsync(request.IncludeArchived);

            _logger.LogInformation("Retrieved {Count} employees", employees.Count());

            return employees;
        }
    }
}