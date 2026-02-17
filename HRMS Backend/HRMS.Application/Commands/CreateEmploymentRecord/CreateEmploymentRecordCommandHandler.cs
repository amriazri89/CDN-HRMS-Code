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

		var record = new EmploymentRecord
		{
			EmploymentRecordId = Guid.NewGuid(),
			EmployeeId = request.EmployeeId,
			EmploymentType = request.EmploymentType,
			Position = request.Position,
			StartDate = request.StartDate,
			EndDate = request.EndDate,
			DailyRate = request.DailyRate,
			IsActive = request.IsActive,

			WorkingDays = request.WorkingDays.Select(day => new EmployeeWorkingDay
			{
				EmployeeWorkingDayId = Guid.NewGuid(),
				DayOfWeek = day
			}).ToList(),

			SkillSets = request.SkillSets.Select(skill => new EmployeeSkillSet
			{
				EmployeeSkillSetId = Guid.NewGuid(),
				SkillName = skill
			}).ToList()
		};

		var created = await _repository.CreateAsync(record);

		_logger.LogInformation("Employment record created: {RecordId}", created.EmploymentRecordId);

		return created;
	}
}