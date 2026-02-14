using System;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces
{
    public interface IPayrollService
    {
        Task<decimal> CalculateSalaryAsync(Guid employeeId, DateTime startDate, DateTime endDate);
    }
}