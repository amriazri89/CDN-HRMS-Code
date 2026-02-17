using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace HRMS.IntegrationTests.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// Custom WebApplicationFactory — replaces services for testing
// ─────────────────────────────────────────────────────────────────────────────
public class HrmsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Use test appsettings
            config.AddJsonFile("appsettings.Test.json", optional: false);
        });

        builder.ConfigureServices(services =>
        {
            // Use InMemory cache for tests (no Redis needed)
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        });

        builder.UseEnvironment("Development");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Integration Tests
// ─────────────────────────────────────────────────────────────────────────────
public class EmployeesControllerIntegrationTests
    : IClassFixture<HrmsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmployeesControllerIntegrationTests(HrmsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── helper: get JWT token ──────────────────────────────────────────────
    private async Task<string> GetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "Admin@123"
        });

        if (!response.IsSuccessStatusCode)
            throw new Exception("Login failed in test setup");

        var content = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<JsonElement>(content, _json);
        return obj.GetProperty("token").GetString()!;
    }

    private void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ── UNAUTHENTICATED ────────────────────────────────────────────────────

    [Fact]
    public async Task GetEmployees_WithoutToken_Returns401()
    {
        // Arrange — no auth header
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET ALL ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEmployees_WithToken_Returns200()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    // ── CREATE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEmployee_ValidData_Returns201()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        var command = new
        {
            name = $"Test User {Guid.NewGuid().ToString()[..8]}",
            nationalNumber = "940110-01-5678",
            contactNumber = "+60123456789",
            position = "Tester",
            address = "Test City",
            dateOfBirth = "1994-01-10"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/employees", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var employee = JsonSerializer.Deserialize<JsonElement>(content, _json);
        employee.GetProperty("name").GetString().Should().Contain("Test User");
    }

    [Fact]
    public async Task CreateEmployee_InvalidName_Returns400()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        var command = new
        {
            name = "",       // ← invalid: empty
            nationalNumber = "940110-01-5678",
            dateOfBirth = "1994-01-10"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/employees", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEmployee_Under18_Returns400()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        var command = new
        {
            name = "Young Person",
            nationalNumber = "940110-01-5678",
            dateOfBirth = DateTime.Now.AddYears(-17).ToString("yyyy-MM-dd") // under 18
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/employees", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET BY ID ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        var fakeId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/employees/{fakeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CREATE THEN GET ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateThenGet_ReturnsCreatedEmployee()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);
        var uniqueName = $"Integration Test {Guid.NewGuid().ToString()[..8]}";

        var command = new
        {
            name = uniqueName,
            nationalNumber = "940110-01-5678",
            contactNumber = "+60123456789",
            position = "Tester",
            address = "Test Address",
            dateOfBirth = "1994-01-10"
        };

        // Act 1: Create
        var createResponse = await _client.PostAsJsonAsync("/api/employees", command);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createContent, _json);
        var employeeId = created.GetProperty("employeeId").GetString();

        // Act 2: Get by ID
        var getResponse = await _client.GetAsync($"/api/employees/{employeeId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var fetched = JsonSerializer.Deserialize<JsonElement>(getContent, _json);
        fetched.GetProperty("name").GetString().Should().Be(uniqueName);
    }

    // ── SEARCH ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_WithKeyword_Returns200()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/employees/search?keyword=Ahmad");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Search_WithoutKeyword_Returns400()
    {
        // Arrange
        var token = await GetTokenAsync();
        SetAuthHeader(token);

        // Act
        var response = await _client.GetAsync("/api/employees/search?keyword=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}