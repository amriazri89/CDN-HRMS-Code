using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Queries.GetActiveEmploymentRecordByEmployeeId;

public class GetActiveEmploymentRecordByEmployeeIdQuery : IRequest<EmploymentRecord?>
{
	public Guid EmployeeId { get; set; }
}