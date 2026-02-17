using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HRMS.IntegrationTests.Controllers;

public class EmploymentRecordsControllerIntegrationTests
    : IClassFixture<HrmsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmploymentRecordsControllerIntegrationTests(HrmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "Admin@123"
        });

        var content = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<JsonElement>(content, _json);
        return obj.GetProperty("token").GetString()!;
    }

    private void SetAuthHeader(string token) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    // helper: create an employee and return its ID
    private async Task<string> CreateEmployeeAsync(string token)
    {
        SetAuthHeader(token);

        var response = await _client.PostAsJsonAsync("/api/employees", new
        {
            name = $"Test Emp {Guid.NewGuid().ToString()[..6]}",
            nationalNumber = "940110-01-5678",
            contactNumber = "+60123456789",
            position = "Tester",
            address = "Test City",
            dateOfBirth = "1994-01-10"
        });

        var content = await response.Content.ReadAsStringAsync();
        var employee = JsonSerializer.Deserialize<JsonElement>(content, _json);
        return employee.GetProperty("employeeId").GetString()!;
    }

    // ── GET ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmployee_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/employmentrecords/employee/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByEmployee_ValidToken_Returns200()
    {
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        var employeeId = await CreateEmployeeAsync(token);
        var response = await _client.GetAsync($"/api/employmentrecords/employee/{employeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── CREATE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRecord_ValidData_Returns201()
    {
        var token = await GetTokenAsync();
        var employeeId = await CreateEmployeeAsync(token);
        SetAuthHeader(token);

        var command = new
        {
            employeeId = employeeId,
            employmentType = "Full-Time",
            position = "Software Engineer",
            startDate = "2024-01-15",
            dailyRate = 250.00,
            isActive = true,
            workingDays = new[] { 1, 2, 3, 4, 5 },  // Mon-Fri
            skillSets = new[] { "C#", "React", "SQL" }
        };

        var response = await _client.PostAsJsonAsync("/api/employmentrecords", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateRecord_InvalidDailyRate_Returns400()
    {
        var token = await GetTokenAsync();
        var employeeId = await CreateEmployeeAsync(token);
        SetAuthHeader(token);

        var command = new
        {
            employeeId = employeeId,
            employmentType = "Full-Time",
            position = "Engineer",
            startDate = "2024-01-15",
            dailyRate = -100,          // ← invalid: negative
            isActive = true,
            workingDays = new[] { 1, 2, 3, 4, 5 }
        };

        var response = await _client.PostAsJsonAsync("/api/employmentrecords", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── CREATE THEN GET ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateThenGet_ReturnsCreatedRecord()
    {
        var token = await GetTokenAsync();
        var employeeId = await CreateEmployeeAsync(token);
        SetAuthHeader(token);

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/employmentrecords", new
        {
            employeeId = employeeId,
            employmentType = "Contract",
            position = "Senior Developer",
            startDate = "2024-01-15",
            dailyRate = 350.00,
            isActive = true,
            workingDays = new[] { 1, 2, 3, 4, 5 },
            skillSets = new[] { "C#", "Azure" }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get
        var getResponse = await _client.GetAsync(
            $"/api/employmentrecords/employee/{employeeId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await getResponse.Content.ReadAsStringAsync();
        var records = JsonSerializer.Deserialize<JsonElement>(content, _json);

        records.GetArrayLength().Should().BeGreaterThan(0);
    }
}