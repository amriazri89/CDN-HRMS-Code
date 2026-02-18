using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Common;

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        // ============================================
        // UPDATED EmployeeRepository.cs - GetPagedAsync
        // Add this to your EmployeeRepository.cs
        // ============================================

        public async Task<PagedResult<Employee>> GetPagedAsync(PaginationParams paginationParams)
        {
            using var connection = CreateConnection();

            // Build dynamic SQL
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Handle search term
            if (!string.IsNullOrEmpty(paginationParams.SearchTerm))
            {
                whereClauses.Add("(Name LIKE @SearchTerm OR EmployeeNumber LIKE @SearchTerm)");
                parameters.Add("SearchTerm", $"%{paginationParams.SearchTerm}%");
            }

            // Handle includeArchived (ADD THIS)
            if (!paginationParams.IncludeArchived)
            {
                whereClauses.Add("IsArchived = 0");
            }

            var whereClause = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            // Get total count
            var countSql = $"SELECT COUNT(*) FROM Employees {whereClause}";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // Get paged data
            var orderBy = paginationParams.SortDescending ? "DESC" : "ASC";
            var offset = (paginationParams.PageNumber - 1) * paginationParams.PageSize;

            var sql = $@"
        SELECT * FROM Employees 
        {whereClause}
        ORDER BY {paginationParams.SortBy} {orderBy}
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", paginationParams.PageSize);

            var employees = await connection.QueryAsync<Employee>(sql, parameters);

            return new PagedResult<Employee>
            {
                Items = employees,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Guid> CreateAsync(Employee employee)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"INSERT INTO Employees (EmployeeId, EmployeeNumber, Name, NationalNumber, 
                        ContactNumber, Position, Address, DateOfBirth, DateCreated, IsArchived)
                        VALUES (@EmployeeId, @EmployeeNumber, @Name, @NationalNumber, 
                        @ContactNumber, @Position, @Address, @DateOfBirth, @DateCreated, @IsArchived)";

            await connection.ExecuteAsync(sql, employee);
            return employee.EmployeeId;
        }

        public async Task<Employee?> GetByIdAsync(Guid employeeId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    e.*,
                    er.*,
                    wd.*,
                    es.*
                FROM Employees e
                LEFT JOIN EmploymentRecords er ON e.EmployeeId = er.EmployeeId
                LEFT JOIN EmployeeWorkingDays wd ON er.EmploymentRecordId = wd.EmploymentRecordId
                LEFT JOIN EmployeeSkillSets es ON er.EmploymentRecordId = es.EmploymentRecordId
                WHERE e.EmployeeId = @EmployeeId";

            var employeeDictionary = new Dictionary<Guid, Employee>();
            var employmentDictionary = new Dictionary<Guid, EmploymentRecord>();

            await connection.QueryAsync<Employee, EmploymentRecord, EmployeeWorkingDay, EmployeeSkillSet, Employee>(
                sql,
                (employee, employment, workingDay, skillSet) =>
                {
                    if (!employeeDictionary.TryGetValue(employee.EmployeeId, out var currentEmployee))
                    {
                        currentEmployee = employee;
                        currentEmployee.EmploymentRecords = new List<EmploymentRecord>();
                        employeeDictionary.Add(employee.EmployeeId, currentEmployee);
                    }

                    if (employment != null)
                    {
                        if (!employmentDictionary.TryGetValue(employment.EmploymentRecordId, out var currentEmployment))
                        {
                            currentEmployment = employment;
                            currentEmployment.WorkingDays = new List<EmployeeWorkingDay>();
                            currentEmployment.SkillSets = new List<EmployeeSkillSet>();
                            currentEmployee.EmploymentRecords.Add(currentEmployment);
                            employmentDictionary.Add(employment.EmploymentRecordId, currentEmployment);
                        }

                        if (workingDay != null && !currentEmployment.WorkingDays.Any(w => w.EmployeeWorkingDayId == workingDay.EmployeeWorkingDayId))
                        {
                            currentEmployment.WorkingDays.Add(workingDay);
                        }

                        if (skillSet != null && !currentEmployment.SkillSets.Any(s => s.EmployeeSkillSetId == skillSet.EmployeeSkillSetId))
                        {
                            currentEmployment.SkillSets.Add(skillSet);
                        }
                    }

                    return currentEmployee;
                },
                new { EmployeeId = employeeId },
                splitOn: "EmploymentRecordId,EmployeeWorkingDayId,EmployeeSkillSetId"
            );

            return employeeDictionary.Values.FirstOrDefault();
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string keyword)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"SELECT * FROM Employees 
                        WHERE IsArchived = 0 
                        AND (EmployeeNumber LIKE @Keyword OR Name LIKE @Keyword)
                        ORDER BY Name";

            return await connection.QueryAsync<Employee>(sql, new { Keyword = $"%{keyword}%" });
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(bool includeArchived = false)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = includeArchived
                ? "SELECT * FROM Employees ORDER BY Name"
                : "SELECT * FROM Employees WHERE IsArchived = 0 ORDER BY Name";

            return await connection.QueryAsync<Employee>(sql);
        }

        public async Task UpdateAsync(Employee employee)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"UPDATE Employees SET 
                        Name = @Name,
                        NationalNumber = @NationalNumber,
                        ContactNumber = @ContactNumber,
                        Position = @Position,
                        Address = @Address,
                        DateOfBirth = @DateOfBirth
                        WHERE EmployeeId = @EmployeeId";

            await connection.ExecuteAsync(sql, employee);
        }

        public async Task DeleteAsync(Guid employeeId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "DELETE FROM Employees WHERE EmployeeId = @EmployeeId";
            await connection.ExecuteAsync(sql, new { EmployeeId = employeeId });
        }

        public async Task ArchiveAsync(Guid employeeId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "UPDATE Employees SET IsArchived = 1 WHERE EmployeeId = @EmployeeId";
            await connection.ExecuteAsync(sql, new { EmployeeId = employeeId });
        }

        public async Task UnarchiveAsync(Guid employeeId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "UPDATE Employees SET IsArchived = 0 WHERE EmployeeId = @EmployeeId";
            await connection.ExecuteAsync(sql, new { EmployeeId = employeeId });
        }
    }
}