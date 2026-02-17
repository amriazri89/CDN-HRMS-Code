using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Queries.GetEmploymentRecordById;

public class GetEmploymentRecordByIdQueryHandler
    : IRequestHandler<GetEmploymentRecordByIdQuery, EmploymentRecord>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<GetEmploymentRecordByIdQueryHandler> _logger;

    public GetEmploymentRecordByIdQueryHandler(
        IEmploymentRecordRepository repository,
        ILogger<GetEmploymentRecordByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmploymentRecord> Handle(
        GetEmploymentRecordByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employment record {RecordId}", request.EmploymentRecordId);

        var record = await _repository.GetByIdAsync(request.EmploymentRecordId);

        if (record == null)
            throw new KeyNotFoundException($"Employment record {request.EmploymentRecordId} not found");

        return record;
    }
}