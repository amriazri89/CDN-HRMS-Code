using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using HRMS.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _repository;
        private readonly IPayrollService _payrollService;

        public EmployeesController(IEmployeeRepository repository, IPayrollService payrollService)
        {
            _repository = repository;
            _payrollService = payrollService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeArchived = false)
        {
            var employees = await _repository.GetAllAsync(includeArchived);
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(employee);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Keyword is required" });

            var results = await _repository.SearchAsync(keyword);
            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeNumber = EmployeeNumberGenerator.Generate(request.Name, request.DateOfBirth),
                Name = request.Name,
                NationalNumber = request.NationalNumber,
                ContactNumber = request.ContactNumber,
                Position = request.Position,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                DateCreated = DateTime.UtcNow,
                IsArchived = false
            };

            var id = await _repository.CreateAsync(employee);
            return CreatedAtAction(nameof(GetById), new { id }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            employee.Name = request.Name;
            employee.NationalNumber = request.NationalNumber;
            employee.ContactNumber = request.ContactNumber;
            employee.Position = request.Position;
            employee.Address = request.Address;
            employee.DateOfBirth = request.DateOfBirth;

            await _repository.UpdateAsync(employee);
            return Ok(new { message = "Employee updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            await _repository.DeleteAsync(id);
            return Ok(new { message = "Employee deleted successfully" });
        }

        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            await _repository.ArchiveAsync(id);
            return Ok(new { message = "Employee archived successfully" });
        }

        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(Guid id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            await _repository.UnarchiveAsync(id);
            return Ok(new { message = "Employee unarchived successfully" });
        }

        [HttpPost("{id}/calculate-salary")]
        public async Task<IActionResult> CalculateSalary(
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var salary = await _payrollService.CalculateSalaryAsync(id, startDate, endDate);
                return Ok(new
                {
                    employeeId = id,
                    startDate,
                    endDate,
                    takeHomePay = salary,
                    currency = "MYR"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
    public record CreateEmployeeRequest(
        string Name,
        string NationalNumber,
        string ContactNumber,
        string Position,
        string Address,
        DateTime DateOfBirth
    );

    public record UpdateEmployeeRequest(
        string Name,
        string NationalNumber,
        string ContactNumber,
        string Position,
        string Address,
        DateTime DateOfBirth
    );
}