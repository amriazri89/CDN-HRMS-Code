using MediatR;

namespace HRMS.Application.Commands.DeactivateEmploymentRecord;

public class DeactivateEmploymentRecordCommand : IRequest<bool>
{
    public Guid EmploymentRecordId { get; set; }
}