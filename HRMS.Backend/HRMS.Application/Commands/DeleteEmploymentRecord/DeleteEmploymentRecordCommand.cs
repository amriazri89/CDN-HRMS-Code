using MediatR;

namespace HRMS.Application.Commands.DeleteEmploymentRecord;

public class DeleteEmploymentRecordCommand : IRequest<bool>
{
    public Guid EmploymentRecordId { get; set; }
}