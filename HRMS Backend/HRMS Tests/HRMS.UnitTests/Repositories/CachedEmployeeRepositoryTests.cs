using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Repositories;
using HRMS.Domain.Entities;
using Xunit;

namespace HRMS.UnitTests.Repositories;

public class CachedEmployeeRepositoryTests
{
    private readonly Mock<IEmployeeRepository> _innerMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<CachedEmployeeRepository>> _loggerMock;
    private readonly CachedEmployeeRepository _cachedRepo;

    public CachedEmployeeRepositoryTests()
    {
        _innerMock = new Mock<IEmployeeRepository>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CachedEmployeeRepository>>();
        _cachedRepo = new CachedEmployeeRepository(
            _innerMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    private static List<Employee> SampleList() =>
    [
        new() { EmployeeId = Guid.NewGuid(), Name = "Ahmad Ali" },
        new() { EmployeeId = Guid.NewGuid(), Name = "Siti Rahimah" }
    ];

    // ── GET ALL ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_CacheHit_ReturnsCachedData_WithoutHittingDb()
    {
        // Arrange — cache has data
        var cached = SampleList();
        _cacheMock
            .Setup(c => c.GetAsync<IEnumerable<Employee>>(It.IsAny<string>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _cachedRepo.GetAllAsync();

        // Assert — DB never called
        result.Should().BeEquivalentTo(cached);
        _innerMock.Verify(r => r.GetAllAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_CacheMiss_HitsDbAndSavesToCache()
    {
        // Arrange — cache is empty
        _cacheMock
            .Setup(c => c.GetAsync<IEnumerable<Employee>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<Employee>?)null);

        _innerMock
            .Setup(r => r.GetAllAsync(false))
            .ReturnsAsync(SampleList());

        // Act
        var result = await _cachedRepo.GetAllAsync();

        // Assert — DB WAS called, and result was saved to cache
        _innerMock.Verify(r => r.GetAllAsync(false), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<Employee>>(),
            It.IsAny<TimeSpan>()), Times.Once);
        result.Should().HaveCount(2);
    }

    // ── GET BY ID ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedEmployee()
    {
        // Arrange
        var id = Guid.NewGuid();
        var employee = new Employee { EmployeeId = id, Name = "Ahmad" };

        _cacheMock
            .Setup(c => c.GetAsync<Employee>($"employee:{id}"))
            .ReturnsAsync(employee);

        // Act
        var result = await _cachedRepo.GetByIdAsync(id);

        // Assert — DB never called
        result.Should().Be(employee);
        _innerMock.Verify(r => r.GetByIdAsync(id), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_HitsDbAndCaches()
    {
        // Arrange
        var id = Guid.NewGuid();
        var employee = new Employee { EmployeeId = id, Name = "Ahmad" };

        _cacheMock
            .Setup(c => c.GetAsync<Employee>(It.IsAny<string>()))
            .ReturnsAsync((Employee?)null);

        _innerMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);

        // Act
        var result = await _cachedRepo.GetByIdAsync(id);

        // Assert
        _innerMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            employee,
            It.IsAny<TimeSpan>()), Times.Once);
        result.Should().Be(employee);
    }

    // ── CREATE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_InvalidatesListCache()
    {
        // Arrange
        var employee = new Employee { EmployeeId = Guid.NewGuid(), Name = "New Employee" };
        _innerMock.Setup(r => r.CreateAsync(employee)).ReturnsAsync(employee.EmployeeId);

        // Act
        await _cachedRepo.CreateAsync(employee);

        // Assert — list cache keys were removed
        _cacheMock.Verify(c => c.RemoveAsync("employees:all:false"), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("employees:all:true"), Times.Once);
    }

    // ── UPDATE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_InvalidatesEmployeeAndListCache()
    {
        // Arrange
        var employee = new Employee { EmployeeId = Guid.NewGuid(), Name = "Updated" };
        _innerMock.Setup(r => r.UpdateAsync(employee)).Returns(Task.CompletedTask);

        // Act
        await _cachedRepo.UpdateAsync(employee);

        // Assert — both the specific employee cache AND list cache cleared
        _cacheMock.Verify(c => c.RemoveAsync($"employee:{employee.EmployeeId}"), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("employees:all:false"), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("employees:all:true"), Times.Once);
    }

    // ── DELETE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_InvalidatesAllRelatedCaches()
    {
        // Arrange
        var id = Guid.NewGuid();
        _innerMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        // Act
        await _cachedRepo.DeleteAsync(id);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync($"employee:{id}"), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("employees:all:false"), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("employees:all:true"), Times.Once);
    }
}