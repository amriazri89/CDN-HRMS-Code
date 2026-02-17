using MediatR;
using HRMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.DeleteEmploymentRecord;

public class DeleteEmploymentRecordCommandHandler
	: IRequestHandler<DeleteEmploymentRecordCommand, bool>
{
	private readonly IEmploymentRecordRepository _repository;
	private readonly ILogger<DeleteEmploymentRecordCommandHandler> _logger;

	public DeleteEmploymentRecordCommandHandler(
		IEmploymentRecordRepository repository,
		ILogger<DeleteEmploymentRecordCommandHandler> logger)
	{
		_repository = repository;
		_logger = logger;
	}

	public async Task<bool> Handle(
		DeleteEmploymentRecordCommand request,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Deleting employment record {RecordId}", request.EmploymentRecordId);

		var existing = await _repository.GetByIdAsync(request.EmploymentRecordId);
		if (existing == null)
			throw new KeyNotFoundException($"Employment record {request.EmploymentRecordId} not found");

		await _repository.DeleteAsync(request.EmploymentRecordId);

		_logger.LogInformation("Employment record deleted: {RecordId}", request.EmploymentRecordId);

		return true;
	}
}