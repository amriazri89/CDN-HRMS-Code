using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using HRMS.Application.Commands.CreateEmploymentRecord;
using HRMS.Application.Commands.UpdateEmploymentRecord;
using HRMS.Application.Commands.DeleteEmploymentRecord;
using HRMS.Application.Queries.GetEmploymentRecordById;
using HRMS.Application.Queries.GetEmploymentRecordsByEmployeeId;

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
    // GET api/employmentrecords/byemployee/{employeeId}
    // Get all employment records for a specific employee
    // ─────────────────────────────────────────────────────
    [HttpGet("byemployee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
    {
        var query = new GetEmploymentRecordsByEmployeeIdQuery { EmployeeId = employeeId };
        var records = await _mediator.Send(query);
        return Ok(records);
    }

    // ─────────────────────────────────────────────────────
    // GET api/employmentrecords/{id}
    // Get single employment record by ID
    // ─────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetEmploymentRecordByIdQuery { EmploymentRecordId = id };
            var record = await _mediator.Send(query);
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
    // Create a new employment record (with working days & skills)
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
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    // ─────────────────────────────────────────────────────
    // PUT api/employmentrecords/{id}
    // Update an existing employment record
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
        catch (ValidationException ex)
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
            var command = new DeleteEmploymentRecordCommand { EmploymentRecordId = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}