using MediatR;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.DeactivateEmploymentRecord;

public class DeactivateEmploymentRecordCommandHandler
    : IRequestHandler<DeactivateEmploymentRecordCommand, bool>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<DeactivateEmploymentRecordCommandHandler> _logger;

    public DeactivateEmploymentRecordCommandHandler(
        IEmploymentRecordRepository repository,
        ILogger<DeactivateEmploymentRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(
        DeactivateEmploymentRecordCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating employment record {RecordId}", request.EmploymentRecordId);

        var record = await _repository.GetByIdAsync(request.EmploymentRecordId);
        if (record == null)
            throw new KeyNotFoundException($"Employment record {request.EmploymentRecordId} not found");

        await _repository.DeactivateAsync(request.EmploymentRecordId);

        _logger.LogInformation("Employment record deactivated: {RecordId}", request.EmploymentRecordId);
        return true;
    }
}