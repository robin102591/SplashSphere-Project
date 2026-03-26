using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollSettings;

public sealed class UpdatePayrollSettingsCommandValidator : AbstractValidator<UpdatePayrollSettingsCommand>
{
    public UpdatePayrollSettingsCommandValidator()
    {
        RuleFor(x => x.CutOffStartDay)
            .InclusiveBetween(0, 6)
            .WithMessage("CutOffStartDay must be between 0 (Sunday) and 6 (Saturday).");
    }
}
