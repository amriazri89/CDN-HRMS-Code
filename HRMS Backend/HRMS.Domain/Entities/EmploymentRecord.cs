using System;
using System.Collections.Generic;

namespace HRMS.Domain.Entities
{
    public class EmploymentRecord
    {
        public Guid EmploymentRecordId { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal DailyRate { get; set; }
        public bool IsActive { get; set; }
        public Guid EmployeeId { get; set; }

        public Employee? Employee { get; set; }
        public ICollection<EmployeeWorkingDay> WorkingDays { get; set; } = new List<EmployeeWorkingDay>();
        public ICollection<EmployeeSkillSet> SkillSets { get; set; } = new List<EmployeeSkillSet>();
    }
}