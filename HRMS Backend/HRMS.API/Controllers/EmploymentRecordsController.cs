using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using FluentValidation;                                              // ← FIX: ValidationException lives here
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

    // ─────────────────────────────────────────────────────
    // GET api/employmentrecords/employee/{employeeId}
    // Get all employment records for an employee
    // ─────────────────────────────────────────────────────
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        var records = await _mediator.Send(
            new GetEmploymentRecordsByEmployeeIdQuery { EmployeeId = employeeId });
        return Ok(records);
    }

    // ─────────────────────────────────────────────────────
    // GET api/employmentrecords/employee/{employeeId}/active
    // Get the active employment record for an employee
    // ─────────────────────────────────────────────────────
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
    }

    // ─────────────────────────────────────────────────────
    // GET api/employmentrecords/{id}
    // Get a single employment record by ID
    // ─────────────────────────────────────────────────────
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
    }

    // ─────────────────────────────────────────────────────
    // POST api/employmentrecords
    // Create a new employment record
    // ─────────────────────────────────────────────────────
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
        catch (ValidationException ex)                              // ← now resolves via FluentValidation
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    // ─────────────────────────────────────────────────────
    // PUT api/employmentrecords/{id}
    // Update an employment record
    // ─────────────────────────────────────────────────────
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
        catch (ValidationException ex)                              // ← now resolves via FluentValidation
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    // ─────────────────────────────────────────────────────
    // DELETE api/employmentrecords/{id}
    // Delete an employment record
    // ─────────────────────────────────────────────────────
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
    }

    // ─────────────────────────────────────────────────────
    // POST api/employmentrecords/{id}/activate
    // Activate a record (deactivates all others for this employee)
    // ─────────────────────────────────────────────────────
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
    }

    // ─────────────────────────────────────────────────────
    // POST api/employmentrecords/{id}/deactivate
    // Deactivate a record
    // ─────────────────────────────────────────────────────
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
    }
}