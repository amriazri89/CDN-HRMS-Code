using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.GetEmployeeById
{
    /// <summary>
    /// Handler that executes GetEmployeeByIdQuery
    /// Uses Dapper repository with complex JOIN query
    /// </summary>
    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Employee?>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;

        public GetEmployeeByIdQueryHandler(
            IEmployeeRepository repository,
            ILogger<GetEmployeeByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Employee?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving employee: {EmployeeId}", request.EmployeeId);

            // Call Dapper repository (executes complex JOIN query)
            // Your existing GetByIdAsync includes:
            // - LEFT JOIN EmploymentRecords
            // - LEFT JOIN EmployeeWorkingDays
            // - LEFT JOIN EmployeeSkillSets
            var employee = await _repository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                _logger.LogWarning("Employee not found: {EmployeeId}", request.EmployeeId);
            else
                _logger.LogInformation("Employee retrieved: {EmployeeNumber}", employee.EmployeeNumber);

            return employee;
        }
    }
}