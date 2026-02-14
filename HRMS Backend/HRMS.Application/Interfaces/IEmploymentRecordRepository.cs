using HRMS.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces
{
    public interface IEmploymentRecordRepository
    {
        // Create
        Task<EmploymentRecord> CreateAsync(EmploymentRecord employmentRecord);

        // Read
        Task<EmploymentRecord> GetByIdAsync(Guid id);
        Task<IEnumerable<EmploymentRecord>> GetByEmployeeIdAsync(Guid employeeId);
        Task<EmploymentRecord> GetActiveByEmployeeIdAsync(Guid employeeId);
        Task<IEnumerable<EmploymentRecord>> GetAllAsync();

        // Update
        Task<EmploymentRecord> UpdateAsync(EmploymentRecord employmentRecord);

        // Delete
        Task<bool> DeleteAsync(Guid id);

        // Activate/Deactivate
        Task<EmploymentRecord> ActivateAsync(Guid id);
        Task<EmploymentRecord> DeactivateAsync(Guid id);
        Task DeactivateAllForEmployeeAsync(Guid employeeId);
    }
}