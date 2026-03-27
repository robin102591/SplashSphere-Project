using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollSettings;

public sealed class UpdatePayrollSettingsCommandValidator : AbstractValidator<UpdatePayrollSettingsCommand>
{
    public UpdatePayrollSettingsCommandValidator()
    {
        RuleFor(x => x.CutOffStartDay)
            .InclusiveBetween(0, 6)
            .WithMessage("CutOffStartDay must be between 0 (Sunday) and 6 (Saturday).");

        RuleFor(x => x.Frequency)
            .InclusiveBetween(1, 2)
            .WithMessage("Frequency must be 1 (Weekly) or 2 (SemiMonthly).");

        RuleFor(x => x.PayReleaseDayOffset)
            .InclusiveBetween(0, 30)
            .WithMessage("PayReleaseDayOffset must be between 0 and 30.");
    }
}
