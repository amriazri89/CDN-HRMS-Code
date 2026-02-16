using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Queries.GetEmployeeById
{
    /// <summary>
    /// Query to get employee by ID
    /// Uses Dapper SELECT with WHERE clause
    /// </summary>
    public record GetEmployeeByIdQuery : IRequest<Employee?>
    {
        public Guid EmployeeId { get; init; }
    }
}