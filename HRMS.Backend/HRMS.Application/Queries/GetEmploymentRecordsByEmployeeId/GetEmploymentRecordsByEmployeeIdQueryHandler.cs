using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.GetEmploymentRecordsByEmployeeId;

public class GetEmploymentRecordsByEmployeeIdQueryHandler
    : IRequestHandler<GetEmploymentRecordsByEmployeeIdQuery, IEnumerable<EmploymentRecord>>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<GetEmploymentRecordsByEmployeeIdQueryHandler> _logger;

    public GetEmploymentRecordsByEmployeeIdQueryHandler(
        IEmploymentRecordRepository repository,
        ILogger<GetEmploymentRecordsByEmployeeIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<EmploymentRecord>> Handle(
        GetEmploymentRecordsByEmployeeIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employment records for Employee {EmployeeId}", request.EmployeeId);

        return await _repository.GetByEmployeeIdAsync(request.EmployeeId);
    }
}