using MediatR;
using HRMS.Domain.Entities;
using HRMS.Domain.Common;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.GetEmployeesPaged
{
	/// <summary>
	/// Handler that executes GetEmployeesPagedQuery
	/// Uses Dapper repository with OFFSET/FETCH for pagination
	/// </summary>
	public class GetEmployeesPagedQueryHandler : IRequestHandler<GetEmployeesPagedQuery, PagedResult<Employee>>
	{
		private readonly IEmployeeRepository _repository;
		private readonly ILogger<GetEmployeesPagedQueryHandler> _logger;

		public GetEmployeesPagedQueryHandler(
			IEmployeeRepository repository,
			ILogger<GetEmployeesPagedQueryHandler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<PagedResult<Employee>> Handle(GetEmployeesPagedQuery request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Retrieving paged employees: Page {PageNumber}, Size {PageSize}",
				request.PageNumber, request.PageSize);

			// Create PaginationParams for Dapper repository
			var paginationParams = new PaginationParams
			{
				PageNumber = request.PageNumber,
				PageSize = request.PageSize,
				SortBy = request.SortBy,
				SortDescending = request.SortDescending,
				SearchTerm = request.SearchTerm,
				IncludeArchived = request.IncludeArchived
			};

			// Call Dapper repository (executes complex SQL with OFFSET/FETCH)
			// Your existing GetPagedAsync includes:
			// - Dynamic WHERE clause building
			// - COUNT query for total
			// - OFFSET/FETCH for pagination
			var result = await _repository.GetPagedAsync(paginationParams);

			_logger.LogInformation("Retrieved page {PageNumber}/{TotalPages}, {Count} employees",
				result.PageNumber, result.TotalPages, result.Items.Count());

			return result;
		}
	}
}