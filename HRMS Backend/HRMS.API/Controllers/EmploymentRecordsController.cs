using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmploymentRecordsController : ControllerBase
    {
        private readonly IEmploymentRecordRepository _employmentRecordRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public EmploymentRecordsController(
            IEmploymentRecordRepository employmentRecordRepository,
            IEmployeeRepository employeeRepository)
        {
            _employmentRecordRepository = employmentRecordRepository;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Create a new employment record for an employee
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmploymentRecordRequest request)
        {
            try
            {
                // Validate employee exists
                var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                // Create employment record
                var employmentRecord = new EmploymentRecord
                {
                    EmploymentRecordId = Guid.NewGuid(),
                    EmployeeId = request.EmployeeId,
                    EmploymentType = request.EmploymentType,
                    Position = request.Position,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DailyRate = request.DailyRate,
                    IsActive = true,
                    WorkingDays = new List<EmployeeWorkingDay>(),
                    SkillSets = new List<EmployeeSkillSet>()
                };

                // Add working days
                if (request.WorkingDays != null && request.WorkingDays.Any())
                {
                    foreach (var dayOfWeek in request.WorkingDays)
                    {
                        employmentRecord.WorkingDays.Add(new EmployeeWorkingDay
                        {
                            EmployeeWorkingDayId = Guid.NewGuid(),
                            EmploymentRecordId = employmentRecord.EmploymentRecordId,
                            DayOfWeek = (DayOfWeek)dayOfWeek
                        });
                    }
                }

                // Add skill sets
                if (request.SkillSets != null && request.SkillSets.Any())
                {
                    foreach (var skillName in request.SkillSets)
                    {
                        employmentRecord.SkillSets.Add(new EmployeeSkillSet
                        {
                            EmployeeSkillSetId = Guid.NewGuid(),
                            EmploymentRecordId = employmentRecord.EmploymentRecordId,
                            SkillName = skillName
                        });
                    }
                }

                // Save using repository
                var created = await _employmentRecordRepository.CreateAsync(employmentRecord);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.EmploymentRecordId },
                    new
                    {
                        employmentRecordId = created.EmploymentRecordId,
                        employeeId = created.EmployeeId,
                        employmentType = created.EmploymentType,
                        position = created.Position,
                        dailyRate = created.DailyRate,
                        startDate = created.StartDate,
                        endDate = created.EndDate,
                        isActive = created.IsActive,
                        workingDays = created.WorkingDays.Select(w => (int)w.DayOfWeek).ToList(),
                        skillSets = created.SkillSets.Select(s => s.SkillName).ToList(),
                        message = "Employment record created successfully"
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all employment records for a specific employee
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(Guid employeeId)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                var employmentRecords = await _employmentRecordRepository.GetByEmployeeIdAsync(employeeId);

                var result = employmentRecords.Select(er => new
                {
                    employmentRecordId = er.EmploymentRecordId,
                    employeeId = er.EmployeeId,
                    employmentType = er.EmploymentType,
                    position = er.Position,
                    dailyRate = er.DailyRate,
                    startDate = er.StartDate,
                    endDate = er.EndDate,
                    isActive = er.IsActive,
                    workingDays = er.WorkingDays.Select(w => new
                    {
                        employeeWorkingDayId = w.EmployeeWorkingDayId,
                        dayOfWeek = (int)w.DayOfWeek,
                        dayName = w.DayOfWeek.ToString()
                    }).ToList(),
                    skillSets = er.SkillSets.Select(s => new
                    {
                        employeeSkillSetId = s.EmployeeSkillSetId,
                        skillName = s.SkillName
                    }).ToList()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get active employment record for an employee
        /// </summary>
        [HttpGet("employee/{employeeId}/active")]
        public async Task<IActionResult> GetActiveByEmployeeId(Guid employeeId)
        {
            try
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                var activeRecord = await _employmentRecordRepository.GetActiveByEmployeeIdAsync(employeeId);
                if (activeRecord == null)
                    return NotFound(new { message = "No active employment record found" });

                return Ok(new
                {
                    employmentRecordId = activeRecord.EmploymentRecordId,
                    employeeId = activeRecord.EmployeeId,
                    employmentType = activeRecord.EmploymentType,
                    position = activeRecord.Position,
                    dailyRate = activeRecord.DailyRate,
                    startDate = activeRecord.StartDate,
                    endDate = activeRecord.EndDate,
                    isActive = activeRecord.IsActive,
                    workingDays = activeRecord.WorkingDays.Select(w => (int)w.DayOfWeek).ToList(),
                    skillSets = activeRecord.SkillSets.Select(s => s.SkillName).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get employment record by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var record = await _employmentRecordRepository.GetByIdAsync(id);
                if (record == null)
                    return NotFound(new { message = "Employment record not found" });

                return Ok(new
                {
                    employmentRecordId = record.EmploymentRecordId,
                    employeeId = record.EmployeeId,
                    employmentType = record.EmploymentType,
                    position = record.Position,
                    dailyRate = record.DailyRate,
                    startDate = record.StartDate,
                    endDate = record.EndDate,
                    isActive = record.IsActive,
                    workingDays = record.WorkingDays.Select(w => (int)w.DayOfWeek).ToList(),
                    skillSets = record.SkillSets.Select(s => s.SkillName).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update employment record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmploymentRecordRequest request)
        {
            try
            {
                var record = await _employmentRecordRepository.GetByIdAsync(id);
                if (record == null)
                    return NotFound(new { message = "Employment record not found" });

                // Update fields
                record.EmploymentType = request.EmploymentType;
                record.Position = request.Position;
                record.DailyRate = request.DailyRate;
                record.StartDate = request.StartDate;
                record.EndDate = request.EndDate;

                // Update working days
                record.WorkingDays = new List<EmployeeWorkingDay>();
                if (request.WorkingDays != null && request.WorkingDays.Any())
                {
                    foreach (var dayOfWeek in request.WorkingDays)
                    {
                        record.WorkingDays.Add(new EmployeeWorkingDay
                        {
                            EmployeeWorkingDayId = Guid.NewGuid(),
                            EmploymentRecordId = record.EmploymentRecordId,
                            DayOfWeek = (DayOfWeek)dayOfWeek
                        });
                    }
                }

                // Update skills
                record.SkillSets = new List<EmployeeSkillSet>();
                if (request.SkillSets != null && request.SkillSets.Any())
                {
                    foreach (var skillName in request.SkillSets)
                    {
                        record.SkillSets.Add(new EmployeeSkillSet
                        {
                            EmployeeSkillSetId = Guid.NewGuid(),
                            EmploymentRecordId = record.EmploymentRecordId,
                            SkillName = skillName
                        });
                    }
                }

                await _employmentRecordRepository.UpdateAsync(record);
                return Ok(new { message = "Employment record updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete employment record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var record = await _employmentRecordRepository.GetByIdAsync(id);
                if (record == null)
                    return NotFound(new { message = "Employment record not found" });

                var deleted = await _employmentRecordRepository.DeleteAsync(id);
                if (!deleted)
                    return BadRequest(new { message = "Failed to delete employment record" });

                return Ok(new { message = "Employment record deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activate employment record (deactivate all others for this employee)
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                var record = await _employmentRecordRepository.ActivateAsync(id);
                if (record == null)
                    return NotFound(new { message = "Employment record not found" });

                return Ok(new { message = "Employment record activated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate employment record
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                var record = await _employmentRecordRepository.DeactivateAsync(id);
                if (record == null)
                    return NotFound(new { message = "Employment record not found" });

                return Ok(new { message = "Employment record deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
    public record CreateEmploymentRecordRequest(
        Guid EmployeeId,
        string EmploymentType,
        string Position,
        DateTime StartDate,
        DateTime? EndDate,
        decimal DailyRate,
        List<int> WorkingDays,
        List<string> SkillSets
    );

    public record UpdateEmploymentRecordRequest(
        string EmploymentType,
        string Position,
        DateTime StartDate,
        DateTime? EndDate,
        decimal DailyRate,
        List<int> WorkingDays,
        List<string> SkillSets
    );
}