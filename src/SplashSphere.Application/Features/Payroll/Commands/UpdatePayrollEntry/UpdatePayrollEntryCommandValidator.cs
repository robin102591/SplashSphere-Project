using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;

public sealed class UpdatePayrollEntryCommandValidator : AbstractValidator<UpdatePayrollEntryCommand>
{
    public UpdatePayrollEntryCommandValidator()
    {
        RuleFor(x => x.EntryId).NotEmpty().WithMessage("Entry ID is required.");

        RuleFor(x => x.Bonuses)
            .GreaterThanOrEqualTo(0).WithMessage("Bonuses cannot be negative.");

        RuleFor(x => x.Deductions)
            .GreaterThanOrEqualTo(0).WithMessage("Deductions cannot be negative.");
    }
}
