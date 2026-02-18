using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Commands.CreateEmploymentRecord;

public class CreateEmploymentRecordCommand : IRequest<EmploymentRecord>
{
    public Guid EmployeeId { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsActive { get; set; }
    public List<DayOfWeek> WorkingDays { get; set; } = new();
    public List<string> SkillSets { get; set; } = new();
}