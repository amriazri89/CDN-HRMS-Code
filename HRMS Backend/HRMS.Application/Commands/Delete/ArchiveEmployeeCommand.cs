using MediatR;

namespace HRMS.Application.Commands.ArchiveEmployee
{
    /// <summary>
    /// Command to archive/unarchive employee
    /// Uses Dapper UPDATE query
    /// </summary>
    public record ArchiveEmployeeCommand : IRequest<bool>
    {
        public Guid EmployeeId { get; init; }
        public bool Archive { get; init; }  // true = archive, false = unarchive
    }
}