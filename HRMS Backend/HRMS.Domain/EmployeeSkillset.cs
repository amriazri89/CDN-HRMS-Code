using System;

namespace HRMS.Domain
{
    public class EmployeeSkillSet
    {
        public Guid EmployeeSkillSetId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public Guid EmploymentRecordId { get; set; }
        public EmploymentRecord? EmploymentRecord { get; set; }
    }
}