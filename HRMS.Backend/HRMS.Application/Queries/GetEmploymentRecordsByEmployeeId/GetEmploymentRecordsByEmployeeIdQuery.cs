using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Queries.GetEmploymentRecordsByEmployeeId;

public class GetEmploymentRecordsByEmployeeIdQuery : IRequest<IEnumerable<EmploymentRecord>>
{
    public Guid EmployeeId { get; set; }
}