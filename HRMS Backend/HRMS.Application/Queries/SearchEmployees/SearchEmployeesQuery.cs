using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Queries.SearchEmployees
{
    /// <summary>
    /// Query to search employees by keyword
    /// Uses Dapper SELECT with LIKE query
    /// </summary>
    public record SearchEmployeesQuery : IRequest<IEnumerable<Employee>>
    {
        public string Keyword { get; init; }
    }
}