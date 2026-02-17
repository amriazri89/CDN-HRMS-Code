using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using HRMS.Infrastructure.Services;
using HRMS.Domain.Entities;
using Xunit;

namespace HRMS.UnitTests.Services;

public class InMemoryCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly InMemoryCacheService _service;

    public InMemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<InMemoryCacheService>>().Object;
        _service = new InMemoryCacheService(_memoryCache, logger);
    }

    // ── SET & GET ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsSameObject()
    {
        // Arrange
        var employee = new Employee { EmployeeId = Guid.NewGuid(), Name = "Ahmad Ali" };

        // Act
        await _service.SetAsync("test:employee", employee, TimeSpan.FromMinutes(5));
        var result = await _service.GetAsync<Employee>("test:employee");

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employee.EmployeeId);
        result.Name.Should().Be("Ahmad Ali");
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        // Act
        var result = await _service.GetAsync<Employee>("does:not:exist");

        // Assert
        result.Should().BeNull();
    }

    // ── REMOVE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveAsync_RemovesItemFromCache()
    {
        // Arrange — set first
        var employee = new Employee { EmployeeId = Guid.NewGuid(), Name = "Test" };
        await _service.SetAsync("remove:test", employee, TimeSpan.FromMinutes(5));

        // Act
        await _service.RemoveAsync("remove:test");
        var result = await _service.GetAsync<Employee>("remove:test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_NonExistentKey_DoesNotThrow()
    {
        // Act & Assert — should not throw
        var act = async () => await _service.RemoveAsync("ghost:key");
        await act.Should().NotThrowAsync();
    }

    // ── REMOVE BY PREFIX ───────────────────────────────────────────────────

    [Fact]
    public async Task RemoveByPrefixAsync_RemovesAllMatchingKeys()
    {
        // Arrange — set 3 keys, 2 with same prefix
        var e1 = new Employee { EmployeeId = Guid.NewGuid(), Name = "One" };
        var e2 = new Employee { EmployeeId = Guid.NewGuid(), Name = "Two" };
        var e3 = new Employee { EmployeeId = Guid.NewGuid(), Name = "Three" };

        await _service.SetAsync("employee:1", e1, TimeSpan.FromMinutes(5));
        await _service.SetAsync("employee:2", e2, TimeSpan.FromMinutes(5));
        await _service.SetAsync("other:key", e3, TimeSpan.FromMinutes(5));

        // Act
        await _service.RemoveByPrefixAsync("employee:");

        // Assert — only employee: keys removed
        var r1 = await _service.GetAsync<Employee>("employee:1");
        var r2 = await _service.GetAsync<Employee>("employee:2");
        var r3 = await _service.GetAsync<Employee>("other:key");

        r1.Should().BeNull();
        r2.Should().BeNull();
        r3.Should().NotBeNull(); // this one should remain
    }

    // ── EXISTS ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var employee = new Employee { EmployeeId = Guid.NewGuid() };
        await _service.SetAsync("exists:test", employee, TimeSpan.FromMinutes(5));

        // Act
        var exists = await _service.ExistsAsync("exists:test");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentKey_ReturnsFalse()
    {
        // Act
        var exists = await _service.ExistsAsync("nope:key");

        // Assert
        exists.Should().BeFalse();
    }

    // ── EXPIRY ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_ExpiredItem_ReturnsNull()
    {
        // Arrange — set with 1 millisecond expiry
        var employee = new Employee { EmployeeId = Guid.NewGuid() };
        await _service.SetAsync("expire:test", employee, TimeSpan.FromMilliseconds(1));

        // Wait for expiry
        await Task.Delay(50);

        // Act
        var result = await _service.GetAsync<Employee>("expire:test");

        // Assert
        result.Should().BeNull();
    }
}