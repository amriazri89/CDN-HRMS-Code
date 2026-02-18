using MediatR;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.DeleteEmployee
{
    /// <summary>
    /// Handler that executes DeleteEmployeeCommand
    /// Uses Dapper repository (executes SQL DELETE)
    /// </summary>
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, bool>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<DeleteEmployeeCommandHandler> _logger;

        public DeleteEmployeeCommandHandler(
            IEmployeeRepository repository,
            ILogger<DeleteEmployeeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting employee: {EmployeeId}", request.EmployeeId);

            // Check if exists (Dapper SELECT)
            var employee = await _repository.GetByIdAsync(request.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found");

            // Delete using Dapper (SQL DELETE)
            await _repository.DeleteAsync(request.EmployeeId);

            _logger.LogInformation("Employee deleted: {EmployeeId}", request.EmployeeId);

            return true;
        }
    }
}