using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.Application.Commands.CreateEmployee;
using HRMS.Application.Commands.UpdateEmployee;
using HRMS.Application.Commands.DeleteEmployee;
using HRMS.Application.Commands.ArchiveEmployee;
using HRMS.Application.Queries.GetEmployees;
using HRMS.Application.Queries.GetEmployeeById;
using HRMS.Application.Queries.GetEmployeesPaged;
using HRMS.Application.Queries.SearchEmployees;
using HRMS.Application.Interfaces;
using System.Text.Json;

namespace HRMS.API.Controllers
{
    /// <summary>
    /// Employees controller using CQRS pattern
    /// All handlers use existing Dapper repositories
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmployeesController> _logger;
        private readonly IPayrollService _payrollService;

        public EmployeesController(
            IMediator mediator,
            ILogger<EmployeesController> logger,
            IPayrollService payrollService)
        {
            _mediator = mediator;
            _logger = logger;
            _payrollService = payrollService;
        }

        // ========== CREATE (CQRS + Dapper) ==========
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command)
        {
            try
            {
                // MediatR → ValidationBehavior → CreateEmployeeCommandHandler → Dapper INSERT
                var employee = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, employee);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { message = "Validation failed", errors });
            }
        }

        // ========== GET ALL (CQRS + Dapper) ==========
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeArchived = false)
        {
            // MediatR → GetEmployeesQueryHandler → Dapper SELECT
            var query = new GetEmployeesQuery { IncludeArchived = includeArchived };
            var employees = await _mediator.Send(query);
            return Ok(employees);
        }

        // ========== GET BY ID (CQRS + Dapper) ==========
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // MediatR → GetEmployeeByIdQueryHandler → Dapper SELECT with JOINs
            var query = new GetEmployeeByIdQuery { EmployeeId = id };
            var employee = await _mediator.Send(query);

            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            return Ok(employee);
        }

        // ========== GET PAGED (CQRS + Dapper OFFSET/FETCH) ==========
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] GetEmployeesPagedQuery query)
        {
            // MediatR → GetEmployeesPagedQueryHandler → Dapper OFFSET/FETCH
            var result = await _mediator.Send(query);

            // Add pagination metadata to headers
            var paginationMetadata = JsonSerializer.Serialize(new
            {
                result.TotalCount,
                result.PageSize,
                result.PageNumber,
                result.TotalPages
            });

            Response.Headers["X-Pagination"] = paginationMetadata;

            return Ok(result);
        }

        // ========== SEARCH (CQRS + Dapper LIKE) ==========
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Keyword is required" });

            // MediatR → SearchEmployeesQueryHandler → Dapper LIKE query
            var query = new SearchEmployeesQuery { Keyword = keyword };
            var results = await _mediator.Send(query);

            return Ok(results);
        }

        // ========== UPDATE (CQRS + Dapper) ==========
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeCommand command)
        {
            try
            {
                if (id != command.EmployeeId)
                    return BadRequest(new { message = "ID mismatch" });

                // MediatR → ValidationBehavior → UpdateEmployeeCommandHandler → Dapper UPDATE
                var employee = await _mediator.Send(command);
                return Ok(new { message = "Employee updated successfully", employee });
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { message = "Validation failed", errors });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ========== DELETE (CQRS + Dapper) ==========
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // MediatR → DeleteEmployeeCommandHandler → Dapper DELETE
                var command = new DeleteEmployeeCommand { EmployeeId = id };
                await _mediator.Send(command);
                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ========== ARCHIVE (CQRS + Dapper) ==========
        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            try
            {
                // MediatR → ArchiveEmployeeCommandHandler → Dapper UPDATE IsArchived
                var command = new ArchiveEmployeeCommand { EmployeeId = id, Archive = true };
                await _mediator.Send(command);
                return Ok(new { message = "Employee archived successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ========== UNARCHIVE (CQRS + Dapper) ==========
        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(Guid id)
        {
            try
            {
                // MediatR → ArchiveEmployeeCommandHandler → Dapper UPDATE IsArchived
                var command = new ArchiveEmployeeCommand { EmployeeId = id, Archive = false };
                await _mediator.Send(command);
                return Ok(new { message = "Employee unarchived successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ========== CALCULATE SALARY (Keep as is - uses PayrollService) ==========
        [HttpPost("{id}/calculate-salary")]
        public async Task<IActionResult> CalculateSalary(
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
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
    }
}