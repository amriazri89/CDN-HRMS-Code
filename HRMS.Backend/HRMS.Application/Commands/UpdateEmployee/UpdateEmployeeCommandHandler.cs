using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.UpdateEmployee
{
    /// <summary>
    /// Handler that executes UpdateEmployeeCommand
    /// Uses Dapper EmployeeRepository (executes SQL UPDATE)
    /// </summary>
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Employee>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

        public UpdateEmployeeCommandHandler(
            IEmployeeRepository repository,
            ILogger<UpdateEmployeeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Employee> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating employee: {EmployeeId}", request.EmployeeId);

            // Get existing employee using Dapper (SELECT query)
            var employee = await _repository.GetByIdAsync(request.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found");

            // Update properties
            employee.Name = request.Name;
            employee.NationalNumber = request.NationalNumber;
            employee.ContactNumber = request.ContactNumber;
            employee.Position = request.Position;
            employee.Address = request.Address;
            employee.DateOfBirth = request.DateOfBirth;

            // Update in database using Dapper (UPDATE query)
            await _repository.UpdateAsync(employee);

            _logger.LogInformation("Employee updated: {EmployeeId}", employee.EmployeeId);

            return employee;
        }
    }
}