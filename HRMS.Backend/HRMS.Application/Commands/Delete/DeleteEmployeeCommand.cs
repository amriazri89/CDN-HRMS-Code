using MediatR;

namespace HRMS.Application.Commands.DeleteEmployee
{
    /// <summary>
    /// Command to delete employee
    /// Uses Dapper DELETE query
    /// </summary>
    public record DeleteEmployeeCommand : IRequest<bool>
    {
        public Guid EmployeeId { get; init; }
    }
}