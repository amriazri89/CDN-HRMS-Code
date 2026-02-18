using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Domain.Entities
{
    public class Employee
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NationalNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsArchived { get; set; }

        public ICollection<EmploymentRecord> EmploymentRecords { get; set; }
            = new List<EmploymentRecord>();

        public EmploymentRecord GetActiveEmployment()
        {
            var employment = EmploymentRecords.FirstOrDefault(x => x.IsActive);
            if (employment == null)
                throw new InvalidOperationException("No active employment record found.");
            return employment;
        }
    }
}