using HRMS.Application.Interfaces;
using HRMS.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public PayrollService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<decimal> CalculateSalaryAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be later than end date.");

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            var employment = employee.GetActiveEmployment();

            decimal total = 0;
            var workingDays = employment.WorkingDays.Select(x => x.DayOfWeek).ToList();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Pay 2x daily rate for working days
                if (workingDays.Contains(date.DayOfWeek))
                {
                    total += employment.DailyRate * 2;
                }

                // Birthday bonus (1x daily rate)
                if (date.Month == employee.DateOfBirth.Month && date.Day == employee.DateOfBirth.Day)
                {
                    total += employment.DailyRate;
                }
            }

            return total;
        }
    }
}