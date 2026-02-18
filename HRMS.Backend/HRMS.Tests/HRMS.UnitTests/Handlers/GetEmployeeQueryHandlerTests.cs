using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using HRMS.Application.Interfaces;
using HRMS.Application.Queries.GetEmployees;
using HRMS.Application.Queries.GetEmployeeById;
using HRMS.Domain.Entities;
using Xunit;

namespace HRMS.UnitTests.Handlers;

// ═══════════════════════════════════════════════════════════
// GET ALL EMPLOYEES
// ═══════════════════════════════════════════════════════════
public class GetEmployeesQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly Mock<ILogger<GetEmployeesQueryHandler>> _loggerMock;
    private readonly GetEmployeesQueryHandler _handler;

    public GetEmployeesQueryHandlerTests()
    {
        _repoMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<GetEmployeesQueryHandler>>();
        _handler = new GetEmployeesQueryHandler(_repoMock.Object, _loggerMock.Object);
    }

    private static List<Employee> SampleEmployees() =>
    [
        new() { EmployeeId = Guid.NewGuid(), Name = "Ahmad Ali",   IsArchived = false },
        new() { EmployeeId = Guid.NewGuid(), Name = "Siti Rahimah", IsArchived = false },
        new() { EmployeeId = Guid.NewGuid(), Name = "David Tan",   IsArchived = true  }
    ];

    [Fact]
    public async Task Handle_ReturnsAllActiveEmployees()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(false)).ReturnsAsync(
            SampleEmployees().Where(e => !e.IsArchived));

        // Act
        var result = await _handler.Handle(
            new GetEmployeesQuery { IncludeArchived = false },
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => !e.IsArchived);
    }

    [Fact]
    public async Task Handle_IncludeArchived_ReturnsAllEmployees()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(true)).ReturnsAsync(SampleEmployees());

        // Act
        var result = await _handler.Handle(
            new GetEmployeesQuery { IncludeArchived = true },
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_NoEmployees_ReturnsEmptyList()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(false))
                 .ReturnsAsync(Enumerable.Empty<Employee>());

        // Act
        var result = await _handler.Handle(
            new GetEmployeesQuery { IncludeArchived = false },
            CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectParameter()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<bool>()))
                 .ReturnsAsync(Enumerable.Empty<Employee>());

        // Act
        await _handler.Handle(
            new GetEmployeesQuery { IncludeArchived = true },
            CancellationToken.None);

        // Assert — must pass true to repository
        _repoMock.Verify(r => r.GetAllAsync(true), Times.Once);
    }
}

// ═══════════════════════════════════════════════════════════
// GET EMPLOYEE BY ID
// ═══════════════════════════════════════════════════════════
public class GetEmployeeByIdQueryHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly Mock<ILogger<GetEmployeeByIdQueryHandler>> _loggerMock;
    private readonly GetEmployeeByIdQueryHandler _handler;

    public GetEmployeeByIdQueryHandlerTests()
    {
        _repoMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<GetEmployeeByIdQueryHandler>>();
        _handler = new GetEmployeeByIdQueryHandler(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingId_ReturnsEmployee()
    {
        // Arrange
        var id = Guid.NewGuid();
        var employee = new Employee { EmployeeId = id, Name = "Ahmad Ali" };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(employee);

        // Act
        var result = await _handler.Handle(
            new GetEmployeeByIdQuery { EmployeeId = id },
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(id);
        result.Name.Should().Be("Ahmad Ali");
    }

    [Fact]
    public async Task Handle_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        // Act
        var result = await _handler.Handle(
            new GetEmployeeByIdQuery { EmployeeId = id },
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        // Act
        await _handler.Handle(
            new GetEmployeeByIdQuery { EmployeeId = id },
            CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }
}