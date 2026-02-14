using Dapper;
using HRMS.Application.Interfaces;
using HRMS.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Repositories
{
    public class EmploymentRecordRepository : IEmploymentRecordRepository
    {
        private readonly string _connectionString;

        public EmploymentRecordRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        // ========== CREATE ==========
        public async Task<EmploymentRecord> CreateAsync(EmploymentRecord employmentRecord)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert employment record
                var sqlEmploymentRecord = @"
                    INSERT INTO EmploymentRecords 
                    (EmploymentRecordId, EmployeeId, EmploymentType, Position, StartDate, EndDate, DailyRate, IsActive)
                    VALUES 
                    (@EmploymentRecordId, @EmployeeId, @EmploymentType, @Position, @StartDate, @EndDate, @DailyRate, @IsActive)";

                await connection.ExecuteAsync(sqlEmploymentRecord, new
                {
                    employmentRecord.EmploymentRecordId,
                    employmentRecord.EmployeeId,
                    employmentRecord.EmploymentType,
                    employmentRecord.Position,
                    employmentRecord.StartDate,
                    employmentRecord.EndDate,
                    employmentRecord.DailyRate,
                    employmentRecord.IsActive
                }, transaction);

                // Insert working days
                if (employmentRecord.WorkingDays != null && employmentRecord.WorkingDays.Any())
                {
                    var sqlWorkingDays = @"
                        INSERT INTO EmployeeWorkingDays 
                        (EmployeeWorkingDayId, EmploymentRecordId, DayOfWeek)
                        VALUES 
                        (@EmployeeWorkingDayId, @EmploymentRecordId, @DayOfWeek)";

                    foreach (var workingDay in employmentRecord.WorkingDays)
                    {
                        await connection.ExecuteAsync(sqlWorkingDays, new
                        {
                            workingDay.EmployeeWorkingDayId,
                            workingDay.EmploymentRecordId,
                            DayOfWeek = (int)workingDay.DayOfWeek
                        }, transaction);
                    }
                }

                // Insert skill sets
                if (employmentRecord.SkillSets != null && employmentRecord.SkillSets.Any())
                {
                    var sqlSkillSets = @"
                        INSERT INTO EmployeeSkillSets 
                        (EmployeeSkillSetId, EmploymentRecordId, SkillName)
                        VALUES 
                        (@EmployeeSkillSetId, @EmploymentRecordId, @SkillName)";

                    foreach (var skillSet in employmentRecord.SkillSets)
                    {
                        await connection.ExecuteAsync(sqlSkillSets, new
                        {
                            skillSet.EmployeeSkillSetId,
                            skillSet.EmploymentRecordId,
                            skillSet.SkillName
                        }, transaction);
                    }
                }

                transaction.Commit();
                return await GetByIdAsync(employmentRecord.EmploymentRecordId);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ========== READ ==========
        public async Task<EmploymentRecord> GetByIdAsync(Guid id)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT * FROM EmploymentRecords WHERE EmploymentRecordId = @Id;
                SELECT * FROM EmployeeWorkingDays WHERE EmploymentRecordId = @Id;
                SELECT * FROM EmployeeSkillSets WHERE EmploymentRecordId = @Id;";

            using var multi = await connection.QueryMultipleAsync(sql, new { Id = id });

            var employmentRecord = await multi.ReadSingleOrDefaultAsync<EmploymentRecord>();
            if (employmentRecord == null) return null;

            var workingDays = (await multi.ReadAsync<EmployeeWorkingDay>()).ToList();
            var skillSets = (await multi.ReadAsync<EmployeeSkillSet>()).ToList();

            employmentRecord.WorkingDays = workingDays;
            employmentRecord.SkillSets = skillSets;

            return employmentRecord;
        }

        public async Task<IEnumerable<EmploymentRecord>> GetByEmployeeIdAsync(Guid employeeId)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT * FROM EmploymentRecords WHERE EmployeeId = @EmployeeId;
                SELECT wd.* FROM EmployeeWorkingDays wd
                INNER JOIN EmploymentRecords er ON wd.EmploymentRecordId = er.EmploymentRecordId
                WHERE er.EmployeeId = @EmployeeId;
                SELECT ss.* FROM EmployeeSkillSets ss
                INNER JOIN EmploymentRecords er ON ss.EmploymentRecordId = er.EmploymentRecordId
                WHERE er.EmployeeId = @EmployeeId;";

            using var multi = await connection.QueryMultipleAsync(sql, new { EmployeeId = employeeId });

            var employmentRecords = (await multi.ReadAsync<EmploymentRecord>()).ToList();
            var workingDays = (await multi.ReadAsync<EmployeeWorkingDay>()).ToList();
            var skillSets = (await multi.ReadAsync<EmployeeSkillSet>()).ToList();

            foreach (var record in employmentRecords)
            {
                record.WorkingDays = workingDays.Where(wd => wd.EmploymentRecordId == record.EmploymentRecordId).ToList();
                record.SkillSets = skillSets.Where(ss => ss.EmploymentRecordId == record.EmploymentRecordId).ToList();
            }

            return employmentRecords;
        }

        public async Task<EmploymentRecord> GetActiveByEmployeeIdAsync(Guid employeeId)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT * FROM EmploymentRecords WHERE EmployeeId = @EmployeeId AND IsActive = 1;
                SELECT wd.* FROM EmployeeWorkingDays wd
                INNER JOIN EmploymentRecords er ON wd.EmploymentRecordId = er.EmploymentRecordId
                WHERE er.EmployeeId = @EmployeeId AND er.IsActive = 1;
                SELECT ss.* FROM EmployeeSkillSets ss
                INNER JOIN EmploymentRecords er ON ss.EmploymentRecordId = er.EmploymentRecordId
                WHERE er.EmployeeId = @EmployeeId AND er.IsActive = 1;";

            using var multi = await connection.QueryMultipleAsync(sql, new { EmployeeId = employeeId });

            var employmentRecord = await multi.ReadSingleOrDefaultAsync<EmploymentRecord>();
            if (employmentRecord == null) return null;

            var workingDays = (await multi.ReadAsync<EmployeeWorkingDay>()).ToList();
            var skillSets = (await multi.ReadAsync<EmployeeSkillSet>()).ToList();

            employmentRecord.WorkingDays = workingDays;
            employmentRecord.SkillSets = skillSets;

            return employmentRecord;
        }

        public async Task<IEnumerable<EmploymentRecord>> GetAllAsync()
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT * FROM EmploymentRecords;
                SELECT * FROM EmployeeWorkingDays;
                SELECT * FROM EmployeeSkillSets;";

            using var multi = await connection.QueryMultipleAsync(sql);

            var employmentRecords = (await multi.ReadAsync<EmploymentRecord>()).ToList();
            var workingDays = (await multi.ReadAsync<EmployeeWorkingDay>()).ToList();
            var skillSets = (await multi.ReadAsync<EmployeeSkillSet>()).ToList();

            foreach (var record in employmentRecords)
            {
                record.WorkingDays = workingDays.Where(wd => wd.EmploymentRecordId == record.EmploymentRecordId).ToList();
                record.SkillSets = skillSets.Where(ss => ss.EmploymentRecordId == record.EmploymentRecordId).ToList();
            }

            return employmentRecords;
        }

        // ========== UPDATE ==========
        public async Task<EmploymentRecord> UpdateAsync(EmploymentRecord employmentRecord)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Update employment record
                var sqlUpdate = @"
                    UPDATE EmploymentRecords SET 
                        EmploymentType = @EmploymentType,
                        Position = @Position,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        DailyRate = @DailyRate,
                        IsActive = @IsActive
                    WHERE EmploymentRecordId = @EmploymentRecordId";

                await connection.ExecuteAsync(sqlUpdate, new
                {
                    employmentRecord.EmploymentRecordId,
                    employmentRecord.EmploymentType,
                    employmentRecord.Position,
                    employmentRecord.StartDate,
                    employmentRecord.EndDate,
                    employmentRecord.DailyRate,
                    employmentRecord.IsActive
                }, transaction);

                // Delete existing working days and skill sets
                await connection.ExecuteAsync(
                    "DELETE FROM EmployeeWorkingDays WHERE EmploymentRecordId = @Id",
                    new { Id = employmentRecord.EmploymentRecordId },
                    transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM EmployeeSkillSets WHERE EmploymentRecordId = @Id",
                    new { Id = employmentRecord.EmploymentRecordId },
                    transaction);

                // Insert new working days
                if (employmentRecord.WorkingDays != null && employmentRecord.WorkingDays.Any())
                {
                    var sqlWorkingDays = @"
                        INSERT INTO EmployeeWorkingDays 
                        (EmployeeWorkingDayId, EmploymentRecordId, DayOfWeek)
                        VALUES 
                        (@EmployeeWorkingDayId, @EmploymentRecordId, @DayOfWeek)";

                    foreach (var workingDay in employmentRecord.WorkingDays)
                    {
                        await connection.ExecuteAsync(sqlWorkingDays, new
                        {
                            workingDay.EmployeeWorkingDayId,
                            workingDay.EmploymentRecordId,
                            DayOfWeek = (int)workingDay.DayOfWeek
                        }, transaction);
                    }
                }

                // Insert new skill sets
                if (employmentRecord.SkillSets != null && employmentRecord.SkillSets.Any())
                {
                    var sqlSkillSets = @"
                        INSERT INTO EmployeeSkillSets 
                        (EmployeeSkillSetId, EmploymentRecordId, SkillName)
                        VALUES 
                        (@EmployeeSkillSetId, @EmploymentRecordId, @SkillName)";

                    foreach (var skillSet in employmentRecord.SkillSets)
                    {
                        await connection.ExecuteAsync(sqlSkillSets, new
                        {
                            skillSet.EmployeeSkillSetId,
                            skillSet.EmploymentRecordId,
                            skillSet.SkillName
                        }, transaction);
                    }
                }

                transaction.Commit();
                return await GetByIdAsync(employmentRecord.EmploymentRecordId);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ========== DELETE ==========
        public async Task<bool> DeleteAsync(Guid id)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete working days
                await connection.ExecuteAsync(
                    "DELETE FROM EmployeeWorkingDays WHERE EmploymentRecordId = @Id",
                    new { Id = id },
                    transaction);

                // Delete skill sets
                await connection.ExecuteAsync(
                    "DELETE FROM EmployeeSkillSets WHERE EmploymentRecordId = @Id",
                    new { Id = id },
                    transaction);

                // Delete employment record
                var result = await connection.ExecuteAsync(
                    "DELETE FROM EmploymentRecords WHERE EmploymentRecordId = @Id",
                    new { Id = id },
                    transaction);

                transaction.Commit();
                return result > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ========== ACTIVATE/DEACTIVATE ==========
        public async Task<EmploymentRecord> ActivateAsync(Guid id)
        {
            using var connection = CreateConnection();

            // Get the employment record to find the employee
            var employmentRecord = await GetByIdAsync(id);
            if (employmentRecord == null) return null;

            // Deactivate all records for this employee
            await DeactivateAllForEmployeeAsync(employmentRecord.EmployeeId);

            // Activate this record
            var sql = "UPDATE EmploymentRecords SET IsActive = 1 WHERE EmploymentRecordId = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });

            return await GetByIdAsync(id);
        }

        public async Task<EmploymentRecord> DeactivateAsync(Guid id)
        {
            using var connection = CreateConnection();

            var sql = "UPDATE EmploymentRecords SET IsActive = 0 WHERE EmploymentRecordId = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });

            return await GetByIdAsync(id);
        }

        public async Task DeactivateAllForEmployeeAsync(Guid employeeId)
        {
            using var connection = CreateConnection();

            var sql = "UPDATE EmploymentRecords SET IsActive = 0 WHERE EmployeeId = @EmployeeId";
            await connection.ExecuteAsync(sql, new { EmployeeId = employeeId });
        }
    }
}