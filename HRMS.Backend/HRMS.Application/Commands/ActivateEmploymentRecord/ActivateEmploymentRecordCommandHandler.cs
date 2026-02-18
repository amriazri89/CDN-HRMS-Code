using MediatR;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.ActivateEmploymentRecord;

public class ActivateEmploymentRecordCommandHandler
    : IRequestHandler<ActivateEmploymentRecordCommand, bool>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<ActivateEmploymentRecordCommandHandler> _logger;

    public ActivateEmploymentRecordCommandHandler(
        IEmploymentRecordRepository repository,
        ILogger<ActivateEmploymentRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(
        ActivateEmploymentRecordCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating employment record {RecordId}", request.EmploymentRecordId);

        var record = await _repository.GetByIdAsync(request.EmploymentRecordId);
        if (record == null)
            throw new KeyNotFoundException($"Employment record {request.EmploymentRecordId} not found");

        await _repository.ActivateAsync(request.EmploymentRecordId);

        _logger.LogInformation("Employment record activated: {RecordId}", request.EmploymentRecordId);
        return true;
    }
}