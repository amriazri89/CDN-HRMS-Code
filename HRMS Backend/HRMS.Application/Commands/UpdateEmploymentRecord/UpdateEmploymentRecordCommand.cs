using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Commands.UpdateEmploymentRecord;

public class UpdateEmploymentRecordCommand : IRequest<EmploymentRecord>
{
    public Guid EmploymentRecordId { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsActive { get; set; }
    public List<DayOfWeek> WorkingDays { get; set; } = new();
    public List<string> SkillSets { get; set; } = new();
}