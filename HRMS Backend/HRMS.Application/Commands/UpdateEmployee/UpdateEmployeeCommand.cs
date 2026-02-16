using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Commands.UpdateEmployee
{
    /// <summary>
    /// Command to update existing employee
    /// Uses Dapper repository in handler
    /// </summary>
    public record UpdateEmployeeCommand : IRequest<Employee>
    {
        public Guid EmployeeId { get; init; }
        public string Name { get; init; }
        public string NationalNumber { get; init; }
        public string ContactNumber { get; init; }
        public string Position { get; init; }
        public string Address { get; init; }
        public DateTime DateOfBirth { get; init; }
    }
}