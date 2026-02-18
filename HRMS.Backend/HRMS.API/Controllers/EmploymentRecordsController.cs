using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using FluentValidation;
using HRMS.Application.Commands.CreateEmploymentRecord;
using HRMS.Application.Commands.UpdateEmploymentRecord;
using HRMS.Application.Commands.DeleteEmploymentRecord;
using HRMS.Application.Commands.ActivateEmploymentRecord;
using HRMS.Application.Commands.DeactivateEmploymentRecord;
using HRMS.Application.Queries.GetEmploymentRecordById;
using HRMS.Application.Queries.GetEmploymentRecordsByEmployeeId;
using HRMS.Application.Queries.GetActiveEmploymentRecordByEmployeeId;

namespace HRMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmploymentRecordsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmploymentRecordsController> _logger;

    public EmploymentRecordsController(
        IMediator mediator,
        ILogger<EmploymentRecordsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // GET api/employmentrecords/employee/{employeeId}
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        try
        {
            var records = await _mediator.Send(
                new GetEmploymentRecordsByEmployeeIdQuery { EmployeeId = employeeId });
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employment records for employee {EmployeeId}", employeeId);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET api/employmentrecords/employee/{employeeId}/active
    [HttpGet("employee/{employeeId:guid}/active")]
    public async Task<IActionResult> GetActiveByEmployee(Guid employeeId)
    {
        try
        {
            var record = await _mediator.Send(
                new GetActiveEmploymentRecordByEmployeeIdQuery { EmployeeId = employeeId });

            if (record == null)
                return NotFound(new { message = "No active employment record found" });

            return Ok(record);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active employment record for employee {EmployeeId}", employeeId);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET api/employmentrecords/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var record = await _mediator.Send(
                new GetEmploymentRecordByIdQuery { EmploymentRecordId = id });
            return Ok(record);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employment record {Id}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST api/employmentrecords
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmploymentRecordCommand command)
    {
        try
        {
            var record = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(GetById),
                new { id = record.EmploymentRecordId },
                record);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new
            {
                message = "Validation failed",
                errors = ex.Errors.Select(e => new
                {
                    propertyName = e.PropertyName,
                    errorMessage = e.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employment record");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT api/employmentrecords/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmploymentRecordCommand command)
    {
        if (id != command.EmploymentRecordId)
            return BadRequest(new { message = "ID mismatch" });

        try
        {
            var record = await _mediator.Send(command);
            return Ok(record);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new
            {
                message = "Validation failed",
                errors = ex.Errors.Select(e => new
                {
                    propertyName = e.PropertyName,
                    errorMessage = e.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employment record {Id}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // DELETE api/employmentrecords/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteEmploymentRecordCommand { EmploymentRecordId = id });
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employment record {Id}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST api/employmentrecords/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            await _mediator.Send(new ActivateEmploymentRecordCommand { EmploymentRecordId = id });
            return Ok(new { message = "Employment record activated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating employment record {Id}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST api/employmentrecords/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            await _mediator.Send(new DeactivateEmploymentRecordCommand { EmploymentRecordId = id });
            return Ok(new { message = "Employment record deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating employment record {Id}", id);
            return StatusCode(500, new { message = ex.Message });
        }
    }
}