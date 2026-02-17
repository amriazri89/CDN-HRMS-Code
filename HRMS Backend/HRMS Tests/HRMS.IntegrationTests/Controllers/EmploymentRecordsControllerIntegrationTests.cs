using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HRMS.IntegrationTests.Controllers;

public class EmploymentRecordsControllerIntegrationTests : IClassFixture<HrmsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public EmploymentRecordsControllerIntegrationTests(HrmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "Admin@123"
        });
        var body = await resp.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<JsonElement>(body, _json);
        return obj.GetProperty("token").GetString()!;
    }

    private void SetAuth(string token) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    private async Task<string> CreateEmployeeAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/employees", new
        {
            name = $"Emp {Guid.NewGuid().ToString()[..6]}",
            nationalNumber = "940110-01-5678",
            contactNumber = "+60123456789",
            position = "Tester",
            address = "Test City",
            dateOfBirth = "1994-01-10"
        });
        var body = await resp.Content.ReadAsStringAsync();
        var emp = JsonSerializer.Deserialize<JsonElement>(body, _json);
        return emp.GetProperty("employeeId").GetString()!;
    }

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
        SetAuth(await GetTokenAsync());
        var employeeId = await CreateEmployeeAsync();
        var response = await _client.GetAsync($"/api/employmentrecords/employee/{employeeId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateRecord_ValidData_Returns201()
    {
        SetAuth(await GetTokenAsync());
        var employeeId = await CreateEmployeeAsync();

        var response = await _client.PostAsJsonAsync("/api/employmentrecords", new
        {
            employeeId = employeeId,
            employmentType = "Full-Time",
            position = "Software Engineer",
            startDate = "2024-01-15",
            dailyRate = 250.00,
            isActive = true,
            workingDays = new[] { 1, 2, 3, 4, 5 },
            skillSets = new[] { "C#", "React", "SQL" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateRecord_NegativeDailyRate_Returns400()
    {
        SetAuth(await GetTokenAsync());
        var employeeId = await CreateEmployeeAsync();

        var response = await _client.PostAsJsonAsync("/api/employmentrecords", new
        {
            employeeId = employeeId,
            employmentType = "Full-Time",
            position = "Engineer",
            startDate = "2024-01-15",
            dailyRate = -100,
            isActive = true,
            workingDays = new[] { 1, 2, 3, 4, 5 }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateThenGet_ReturnsRecord()
    {
        SetAuth(await GetTokenAsync());
        var employeeId = await CreateEmployeeAsync();

        var createResp = await _client.PostAsJsonAsync("/api/employmentrecords", new
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
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResp = await _client.GetAsync($"/api/employmentrecords/employee/{employeeId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await getResp.Content.ReadAsStringAsync();
        var records = JsonSerializer.Deserialize<JsonElement>(content, _json);
        records.GetArrayLength().Should().BeGreaterThan(0);
    }
}