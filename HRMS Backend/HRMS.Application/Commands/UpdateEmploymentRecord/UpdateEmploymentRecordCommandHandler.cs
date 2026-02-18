using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.CreateEmploymentRecord;

public class CreateEmploymentRecordCommandHandler
    : IRequestHandler<CreateEmploymentRecordCommand, EmploymentRecord>
{
    private readonly IEmploymentRecordRepository _repository;
    private readonly ILogger<CreateEmploymentRecordCommandHandler> _logger;

    public CreateEmploymentRecordCommandHandler(
        IEmploymentRecordRepository repository,
        ILogger<CreateEmploymentRecordCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmploymentRecord> Handle(
        CreateEmploymentRecordCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating employment record for Employee {EmployeeId}", request.EmployeeId);

        // ✅ Generate the parent ID first
        var employmentRecordId = Guid.NewGuid();

        var record = new EmploymentRecord
        {
            EmploymentRecordId = employmentRecordId,  // ← Set parent ID
            EmployeeId = request.EmployeeId,
            EmploymentType = request.EmploymentType,
            Position = request.Position,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DailyRate = request.DailyRate,
            IsActive = request.IsActive,

            // ✅ FIX - Set EmploymentRecordId on each child BEFORE saving
            WorkingDays = request.WorkingDays.Select(day => new EmployeeWorkingDay
            {
                EmployeeWorkingDayId = Guid.NewGuid(),
                EmploymentRecordId = employmentRecordId,  // ← SET PARENT ID!
                DayOfWeek = day
            }).ToList(),

            SkillSets = request.SkillSets.Select(skill => new EmployeeSkillSet
            {
                EmployeeSkillSetId = Guid.NewGuid(),
                EmploymentRecordId = employmentRecordId,  // ← SET PARENT ID!
                SkillName = skill
            }).ToList()
        };

        var created = await _repository.CreateAsync(record);

        _logger.LogInformation("Employment record created: {RecordId}", created.EmploymentRecordId);

        return created;
    }
}