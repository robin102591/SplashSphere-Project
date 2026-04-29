using FluentValidation;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateDisplaySetting;

public sealed class UpdateDisplaySettingCommandValidator : AbstractValidator<UpdateDisplaySettingCommand>
{
    public UpdateDisplaySettingCommandValidator()
    {
        // Promo rotation: 3-60 seconds. Anything shorter is jarring; longer
        // means an idle counter sees the same message for too long.
        RuleFor(x => x.PromoRotationSeconds)
            .InclusiveBetween(3, 60);

        // Completion hold: 3-30 seconds. Long enough to read; short enough to
        // free the screen for the next customer.
        RuleFor(x => x.CompletionHoldSeconds)
            .InclusiveBetween(3, 30);

        RuleForEach(x => x.PromoMessages)
            .NotEmpty()
            .MaximumLength(200);

        // Cap total list to keep the row size sane.
        RuleFor(x => x.PromoMessages)
            .Must(list => list.Count <= 20)
            .WithMessage("Cannot define more than 20 promo messages.");
    }
}
