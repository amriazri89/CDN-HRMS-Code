using HRMS.Domain.Entities;
using HRMS.Domain.Common;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<PagedResult<Employee>> GetPagedAsync(PaginationParams paginationParams);

        Task<Guid> CreateAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Guid employeeId);
        Task ArchiveAsync(Guid employeeId);
        Task UnarchiveAsync(Guid employeeId);
        Task<Employee?> GetByIdAsync(Guid employeeId);
        Task<IEnumerable<Employee>> SearchAsync(string keyword);
        Task<IEnumerable<Employee>> GetAllAsync(bool includeArchived = false);
    }
}