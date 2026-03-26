using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;

public sealed class UpdatePayrollEntryCommandValidator : AbstractValidator<UpdatePayrollEntryCommand>
{
    public UpdatePayrollEntryCommandValidator()
    {
        RuleFor(x => x.EntryId).NotEmpty().WithMessage("Entry ID is required.");
    }
}
