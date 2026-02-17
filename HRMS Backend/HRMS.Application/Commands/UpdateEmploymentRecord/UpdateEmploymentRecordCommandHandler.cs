using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.UpdateEmploymentRecord;

public class UpdateEmploymentRecordCommandHandler
    : IRequestHandler<UpdateEmploymentRecordCommand, EmploymentRecord>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<UpdateEmploymentRecordCommandHandler> _logger;

    public UpdateEmploymentRecordCommandHandler(
        IEmploymentRecordRepository repository,
        ILogger<UpdateEmploymentRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmploymentRecord> Handle(
        UpdateEmploymentRecordCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating employment record {RecordId}", request.EmploymentRecordId);

        var existing = await _repository.GetByIdAsync(request.EmploymentRecordId);
        if (existing == null)
            throw new KeyNotFoundException($"Employment record {request.EmploymentRecordId} not found");

        existing.EmploymentType = request.EmploymentType;
        existing.Position = request.Position;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.DailyRate = request.DailyRate;
        existing.IsActive = request.IsActive;

        existing.WorkingDays = request.WorkingDays.Select(day => new EmployeeWorkingDay
        {
            EmployeeWorkingDayId = Guid.NewGuid(),
            EmploymentRecordId = existing.EmploymentRecordId,
            DayOfWeek = day
        }).ToList();

        existing.SkillSets = request.SkillSets.Select(skill => new EmployeeSkillSet
        {
            EmployeeSkillSetId = Guid.NewGuid(),
            EmploymentRecordId = existing.EmploymentRecordId,
            SkillName = skill
        }).ToList();

        await _repository.UpdateAsync(existing);

        _logger.LogInformation("Employment record updated: {RecordId}", existing.EmploymentRecordId);

        return await _repository.GetByIdAsync(existing.EmploymentRecordId);
    }
}