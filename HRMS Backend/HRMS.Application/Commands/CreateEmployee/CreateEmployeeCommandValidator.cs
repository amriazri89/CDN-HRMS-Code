using FluentValidation;

namespace HRMS.Application.Commands.CreateEmployee
{
    /// <summary>
    /// Validator for CreateEmployeeCommand
    /// Runs automatically before handler via ValidationBehavior
    /// </summary>
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces");

            RuleFor(x => x.NationalNumber)
                .NotEmpty().WithMessage("National Number is required")
                .Matches(@"^\d{6}-\d{2}-\d{4}$")
                .WithMessage("National Number must be in format YYMMDD-XX-XXXX (e.g., 940110-01-5678)");

            RuleFor(x => x.ContactNumber)
                .Matches(@"^\+?60\d{9,10}$")
                .When(x => !string.IsNullOrEmpty(x.ContactNumber))
                .WithMessage("Contact Number must be a valid Malaysian phone number (+60XXXXXXXXX)");

            RuleFor(x => x.Position)
                .MaximumLength(50).WithMessage("Position cannot exceed 50 characters");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of Birth is required")
                .LessThan(DateTime.Now).WithMessage("Date of Birth must be in the past")
                .Must(BeAtLeast18YearsOld).WithMessage("Employee must be at least 18 years old");
        }

        private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
        {
            var age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}