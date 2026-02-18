using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HRMS.IntegrationTests.Controllers;

public class HrmsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // ✅ No config overrides needed
        // Let the app use its own appsettings.json and appsettings.Development.json
        // This ensures JWT key, DB connection, everything matches exactly

        builder.ConfigureServices(services =>
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        });
    }
}

public class EmployeesControllerIntegrationTests : IClassFixture<HrmsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public EmployeesControllerIntegrationTests(HrmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var resp = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admintest",
            password = "Admin@123"
        });

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Login failed: {resp.StatusCode} - {await resp.Content.ReadAsStringAsync()}");

        var body = await resp.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<JsonElement>(body, _json);
        return obj.GetProperty("token").GetString()!;
    }

    private void SetAuth(string token) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    [Fact]
    public async Task GetEmployees_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/employees");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEmployees_WithToken_Returns200()
    {
        SetAuth(await GetTokenAsync());
        var response = await _client.GetAsync("/api/employees");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateEmployee_ValidData_Returns201()
    {
        SetAuth(await GetTokenAsync());
        var response = await _client.PostAsJsonAsync("/api/employees", new
        {
            name = $"Test User {Guid.NewGuid().ToString()[..8]}",
            nationalNumber = "940110-01-5678",
            contactNumber = "+60123456789",
            position = "Tester",
            address = "Test City",
            dateOfBirth = "1994-01-10"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateEmployee_EmptyName_Returns400()
    {
        SetAuth(await GetTokenAsync());
        var response = await _client.PostAsJsonAsync("/api/employees", new
        {
            name = "",
            nationalNumber = "940110-01-5678",
            dateOfBirth = "1994-01-10"
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEmployee_Under18_Returns400()
    {
        SetAuth(await GetTokenAsync());
        var response = await _client.PostAsJsonAsync("/api/employees", new
        {
            name = "Young Person",
            nationalNumber = "940110-01-5678",
            dateOfBirth = DateTime.Now.AddYears(-17).ToString("yyyy-MM-dd")
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        SetAuth(await GetTokenAsync());
        var response = await _client.GetAsync($"/api/employees/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateThenGet_ReturnsCreatedEmployee()
    {
        SetAuth(await GetTokenAsync());
        var uniqueName = $"Integration {Guid.NewGuid().ToString()[..8]}";

        var createResp = await _client.PostAsJsonAsync("/api/employees", new
        {
            name = uniqueName,
            nationalNumber = "940110-01-5678",
            contactNumber = "+60123456789",
            position = "Tester",
            address = "Test Address",
            dateOfBirth = "1994-01-10"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await createResp.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(body, _json);
        var employeeId = created.GetProperty("employeeId").GetString();

        var getResp = await _client.GetAsync($"/api/employees/{employeeId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getBody = await getResp.Content.ReadAsStringAsync();
        var fetched = JsonSerializer.Deserialize<JsonElement>(getBody, _json);
        fetched.GetProperty("name").GetString().Should().Be(uniqueName);
    }

    [Fact]
    public async Task Search_WithKeyword_Returns200()
    {
        SetAuth(await GetTokenAsync());
        var response = await _client.GetAsync("/api/employees/search?keyword=Test");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}