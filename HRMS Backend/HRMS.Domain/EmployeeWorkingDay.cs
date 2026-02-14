using System;

namespace HRMS.Domain
{
    public class EmployeeWorkingDay
    {
        public Guid EmployeeWorkingDayId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public Guid EmploymentRecordId { get; set; }
        public EmploymentRecord? EmploymentRecord { get; set; }
    }
}