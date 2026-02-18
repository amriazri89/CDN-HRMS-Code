using MediatR;
using HRMS.Domain.Entities;
using HRMS.Domain.Common;

namespace HRMS.Application.Queries.GetEmployeesPaged
{
    /// <summary>
    /// Query to get paginated employees
    /// Uses Dapper OFFSET/FETCH query
    /// </summary>
    public record GetEmployeesPagedQuery : IRequest<PagedResult<Employee>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string SortBy { get; init; } = "Name";
        public bool SortDescending { get; init; } = false;
        public string? SearchTerm { get; init; }
        public bool IncludeArchived { get; init; } = false;
    }
}