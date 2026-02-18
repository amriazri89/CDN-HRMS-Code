using MediatR;

namespace HRMS.Application.Commands.ActivateEmploymentRecord;

public class ActivateEmploymentRecordCommand : IRequest<bool>
{
    public Guid EmploymentRecordId { get; set; }
}