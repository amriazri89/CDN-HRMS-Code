using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Queries.GetEmployees
{
    /// <summary>
    /// Query to get all employees
    /// Uses Dapper SELECT query
    /// </summary>
    public record GetEmployeesQuery : IRequest<IEnumerable<Employee>>
    {
        public bool IncludeArchived { get; init; }
    }
}