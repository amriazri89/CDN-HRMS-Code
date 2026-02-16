using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Commands.CreateEmployee
{
	public record CreateEmployeeCommand : IRequest<Employee>
	{
		public string Name { get; init; }
		public string NationalNumber { get; init; }
		public string ContactNumber { get; init; }
		public string Position { get; init; }
		public string Address { get; init; }
		public DateTime DateOfBirth { get; init; }
	}
}