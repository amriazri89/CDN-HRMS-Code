using FluentValidation;

namespace HRMS.Application.Commands.UpdateEmploymentRecord;

public class UpdateEmploymentRecordCommandValidator
    : AbstractValidator<UpdateEmploymentRecordCommand>
{
    public UpdateEmploymentRecordCommandValidator()
    {
        RuleFor(x => x.EmploymentRecordId)
            .NotEmpty()
            .WithMessage("Employment record ID is required");

        RuleFor(x => x.EmploymentType)
            .NotEmpty()
            .WithMessage("Employment type is required")
            .MaximumLength(50)
            .WithMessage("Employment type cannot exceed 50 characters");

        RuleFor(x => x.Position)
            .NotEmpty()
            .WithMessage("Position is required")
            .MaximumLength(100)
            .WithMessage("Position cannot exceed 100 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.DailyRate)
            .GreaterThan(0)
            .WithMessage("Daily rate must be greater than 0");

        RuleFor(x => x.WorkingDays)
            .NotEmpty()
            .WithMessage("At least one working day is required")
            .Must(days => days.Distinct().Count() == days.Count)
            .WithMessage("Working days cannot have duplicates");

        RuleForEach(x => x.SkillSets)
            .NotEmpty()
            .WithMessage("Skill name cannot be empty")
            .MaximumLength(100)
            .WithMessage("Skill name cannot exceed 100 characters");
    }
}