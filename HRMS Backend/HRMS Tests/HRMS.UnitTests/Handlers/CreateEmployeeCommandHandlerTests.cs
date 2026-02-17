using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using HRMS.Application.Commands.CreateEmployee;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using Xunit;

namespace HRMS.UnitTests.Handlers;

public class CreateEmployeeCommandHandlerTests
{
    // ── mocks ──────────────────────────────────────────────────────────────
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly Mock<ILogger<CreateEmployeeCommandHandler>> _loggerMock;
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _repoMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<CreateEmployeeCommandHandler>>();
        _handler = new CreateEmployeeCommandHandler(_repoMock.Object, _loggerMock.Object);
    }

    // ── helpers ────────────────────────────────────────────────────────────
    private static CreateEmployeeCommand ValidCommand() => new()
    {
        Name = "Ahmad Ali",
        NationalNumber = "940110-01-5678",
        ContactNumber = "+60123456789",
        Position = "Engineer",
        Address = "Kuala Lumpur",
        DateOfBirth = new DateTime(1994, 1, 10)
    };

    // ── tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_ReturnsEmployee()
    {
        // Arrange
        var command = ValidCommand();
        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Employee>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Ahmad Ali");
        result.Position.Should().Be("Engineer");
        result.IsArchived.Should().BeFalse();
        result.EmployeeId.Should().NotBeEmpty();
        result.EmployeeNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryOnce()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Employee>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert — repository was called exactly once
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsDateCreated()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Employee>()))
            .ReturnsAsync(Guid.NewGuid());

        var before = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.DateCreated.Should().BeOnOrAfter(before);
        result.DateCreated.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesEmployeeNumber()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Employee>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert — employee number must be generated
        result.EmployeeNumber.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_RepositoryThrows_ExceptionBubblesUp()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Employee>()))
            .ThrowsAsync(new Exception("DB connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(ValidCommand(), CancellationToken.None));
    }
}