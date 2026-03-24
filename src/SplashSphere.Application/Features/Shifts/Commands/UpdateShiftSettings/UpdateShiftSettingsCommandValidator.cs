using FluentValidation;

namespace SplashSphere.Application.Features.Shifts.Commands.UpdateShiftSettings;

public sealed class UpdateShiftSettingsCommandValidator : AbstractValidator<UpdateShiftSettingsCommand>
{
    public UpdateShiftSettingsCommandValidator()
    {
        RuleFor(x => x.DefaultOpeningFund).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AutoApproveThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FlagThreshold)
            .GreaterThan(x => x.AutoApproveThreshold)
            .WithMessage("FlagThreshold must be greater than AutoApproveThreshold.");
        RuleFor(x => x.LockTimeoutMinutes)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(60);
        RuleFor(x => x.MaxPinAttempts)
            .InclusiveBetween(1, 20);
    }
}
