using MediatR;
using HRMS.Domain.Entities;
using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Commands.CreateEmployee
{
	/// <summary>
	/// Handler that executes CreateEmployeeCommand
	/// Uses existing Dapper EmployeeRepository
	/// </summary>
	public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Employee>
	{
		private readonly IEmployeeRepository _repository;
		private readonly ILogger<CreateEmployeeCommandHandler> _logger;

		public CreateEmployeeCommandHandler(
			IEmployeeRepository repository,
			ILogger<CreateEmployeeCommandHandler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Employee> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Creating employee: {Name}", request.Name);

			// Create employee entity
			var employee = new Employee
			{
				EmployeeId = Guid.NewGuid(),
				Name = request.Name,
				NationalNumber = request.NationalNumber,
				ContactNumber = request.ContactNumber,
				Position = request.Position,
				Address = request.Address,
				DateOfBirth = request.DateOfBirth,
				DateCreated = DateTime.UtcNow,
				IsArchived = false
			};

			// Generate employee number
			employee.EmployeeNumber = EmployeeNumberGenerator.Generate(
				employee.Name,
				employee.DateOfBirth
			);

			// Call Dapper repository (which executes SQL INSERT)
			await _repository.CreateAsync(employee);

			_logger.LogInformation("Employee created: {EmployeeNumber}", employee.EmployeeNumber);

			return employee;
		}
	}
}