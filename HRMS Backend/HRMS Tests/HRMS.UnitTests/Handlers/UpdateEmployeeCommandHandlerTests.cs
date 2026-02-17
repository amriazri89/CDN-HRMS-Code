using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using HRMS.Application.Commands.UpdateEmployee;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using Xunit;

namespace HRMS.UnitTests.Handlers;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly Mock<ILogger<UpdateEmployeeCommandHandler>> _loggerMock;
    private readonly UpdateEmployeeCommandHandler _handler;

    public UpdateEmployeeCommandHandlerTests()
    {
        _repoMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<UpdateEmployeeCommandHandler>>();
        _handler = new UpdateEmployeeCommandHandler(_repoMock.Object, _loggerMock.Object);
    }

    private static Employee ExistingEmployee(Guid id) => new()
    {
        EmployeeId = id,
        EmployeeNumber = "EMP-001",
        Name = "Old Name",
        NationalNumber = "940110-01-5678",
        ContactNumber = "+60123456789",
        Position = "Junior Engineer",
        Address = "Penang",
        DateOfBirth = new DateTime(1994, 1, 10),
        IsArchived = false
    };

    [Fact]
    public async Task Handle_ExistingEmployee_UpdatesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            Name = "New Name",
            NationalNumber = "940110-01-5678",
            ContactNumber = "+60123456789",
            Position = "Senior Engineer",
            Address = "Kuala Lumpur",
            DateOfBirth = new DateTime(1994, 1, 10)
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(ExistingEmployee(id));
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("New Name");
        result.Position.Should().Be("Senior Engineer");
        result.Address.Should().Be("Kuala Lumpur");
    }

    [Fact]
    public async Task Handle_ExistingEmployee_CallsUpdateOnce()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(ExistingEmployee(id));
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

        var command = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            Name = "Updated",
            NationalNumber = "940110-01-5678",
            DateOfBirth = new DateTime(1994, 1, 10)
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var command = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            Name = "Test",
            DateOfBirth = new DateTime(1994, 1, 10)
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_NeverCallsUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var command = new UpdateEmployeeCommand { EmployeeId = id, Name = "Test" };

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert — update should NEVER be called if employee not found
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Never);
    }
}