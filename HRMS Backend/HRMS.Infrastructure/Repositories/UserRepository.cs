using HRMS.Application.Interfaces;
using HRMS.Domain;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found");
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM Users WHERE Username = @Username";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM Users WHERE UserId = @UserId";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        public async Task<Guid> CreateAsync(User user)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = @"INSERT INTO Users (UserId, Username, PasswordHash, CreatedAt, IsActive)
                        VALUES (@UserId, @Username, @PasswordHash, @CreatedAt, @IsActive)";

            await connection.ExecuteAsync(sql, user);
            return user.UserId;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }
    }
}