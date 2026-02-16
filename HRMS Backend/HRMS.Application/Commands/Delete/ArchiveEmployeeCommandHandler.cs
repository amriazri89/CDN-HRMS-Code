using MediatR;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.ArchiveEmployee
{
    /// <summary>
    /// Handler that executes ArchiveEmployeeCommand
    /// Uses Dapper repository (executes SQL UPDATE IsArchived)
    /// </summary>
    public class ArchiveEmployeeCommandHandler : IRequestHandler<ArchiveEmployeeCommand, bool>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<ArchiveEmployeeCommandHandler> _logger;

        public ArchiveEmployeeCommandHandler(
            IEmployeeRepository repository,
            ILogger<ArchiveEmployeeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(ArchiveEmployeeCommand request, CancellationToken cancellationToken)
        {
            var action = request.Archive ? "Archiving" : "Unarchiving";
            _logger.LogInformation("{Action} employee: {EmployeeId}", action, request.EmployeeId);

            // Check if exists (Dapper SELECT)
            var employee = await _repository.GetByIdAsync(request.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID {request.EmployeeId} not found");

            // Archive or unarchive using Dapper (SQL UPDATE)
            if (request.Archive)
                await _repository.ArchiveAsync(request.EmployeeId);
            else
                await _repository.UnarchiveAsync(request.EmployeeId);

            _logger.LogInformation("Employee {Action}d: {EmployeeId}", action.ToLower(), request.EmployeeId);

            return true;
        }
    }
}