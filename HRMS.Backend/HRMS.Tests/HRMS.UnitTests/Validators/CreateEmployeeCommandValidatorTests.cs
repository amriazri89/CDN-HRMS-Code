using FluentAssertions;
using FluentValidation.TestHelper;
using HRMS.Application.Commands.CreateEmployee;
using Xunit;

namespace HRMS.UnitTests.Validators;

public class CreateEmployeeCommandValidatorTests
{
    private readonly CreateEmployeeCommandValidator _validator = new();

    // ── helper ─────────────────────────────────────────────────────────────
    private static CreateEmployeeCommand ValidCommand() => new()
    {
        Name = "Ahmad Ali",
        NationalNumber = "940110-01-5678",
        ContactNumber = "+60123456789",
        Position = "Engineer",
        Address = "Kuala Lumpur",
        DateOfBirth = new DateTime(1994, 1, 10)
    };

    // ── NAME ───────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Empty_ShouldFail()
    {
        var cmd = ValidCommand() with { Name = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Name_TooShort_ShouldFail()
    {
        var cmd = ValidCommand() with { Name = "A" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WithNumbers_ShouldFail()
    {
        var cmd = ValidCommand() with { Name = "Ahmad123" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name can only contain letters and spaces");
    }

    [Fact]
    public void Name_Valid_ShouldPass()
    {
        var cmd = ValidCommand() with { Name = "Ahmad Ali" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── NATIONAL NUMBER ────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]              // empty
    [InlineData("123456")]        // too short
    [InlineData("9401100-1-5678")]// wrong format
    [InlineData("AABBCC-DD-EEEE")]// letters
    public void NationalNumber_Invalid_ShouldFail(string value)
    {
        var cmd = ValidCommand() with { NationalNumber = value };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.NationalNumber);
    }

    [Fact]
    public void NationalNumber_ValidFormat_ShouldPass()
    {
        var cmd = ValidCommand() with { NationalNumber = "940110-01-5678" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.NationalNumber);
    }

    // ── CONTACT NUMBER ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("0123456789")]    // no +60 prefix
    [InlineData("+6012345")]      // too short
    [InlineData("+1234567890")]   // wrong country code
    public void ContactNumber_Invalid_ShouldFail(string value)
    {
        var cmd = ValidCommand() with { ContactNumber = value };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ContactNumber);
    }

    [Theory]
    [InlineData("+60123456789")]
    [InlineData("+601234567890")]
    public void ContactNumber_ValidMalaysian_ShouldPass(string value)
    {
        var cmd = ValidCommand() with { ContactNumber = value };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.ContactNumber);
    }

    [Fact]
    public void ContactNumber_Empty_ShouldPass()
    {
        // ContactNumber is optional
        var cmd = ValidCommand() with { ContactNumber = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.ContactNumber);
    }

    // ── DATE OF BIRTH ──────────────────────────────────────────────────────

    [Fact]
    public void DateOfBirth_FutureDate_ShouldFail()
    {
        var cmd = ValidCommand() with { DateOfBirth = DateTime.Now.AddDays(1) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_Under18_ShouldFail()
    {
        var cmd = ValidCommand() with { DateOfBirth = DateTime.Now.AddYears(-17) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
              .WithErrorMessage("Employee must be at least 18 years old");
    }

    [Fact]
    public void DateOfBirth_Over18_ShouldPass()
    {
        var cmd = ValidCommand() with { DateOfBirth = DateTime.Now.AddYears(-25) };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    // ── FULL COMMAND ───────────────────────────────────────────────────────

    [Fact]
    public void ValidCommand_ShouldPassAllRules()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}