using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.GetActiveEmploymentRecordByEmployeeId;

public class GetActiveEmploymentRecordByEmployeeIdQueryHandler
    : IRequestHandler<GetActiveEmploymentRecordByEmployeeIdQuery, EmploymentRecord?>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<GetActiveEmploymentRecordByEmployeeIdQueryHandler> _logger;

    public GetActiveEmploymentRecordByEmployeeIdQueryHandler(
        IEmploymentRecordRepository repository,
        ILogger<GetActiveEmploymentRecordByEmployeeIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmploymentRecord?> Handle(
        GetActiveEmploymentRecordByEmployeeIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active employment record for Employee {EmployeeId}", request.EmployeeId);

        return await _repository.GetActiveByEmployeeIdAsync(request.EmployeeId);
    }
}