using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.CreatePricingModifier;

public sealed class CreatePricingModifierCommandValidator
    : AbstractValidator<CreatePricingModifierCommand>
{
    public CreatePricingModifierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Value must be greater than zero.");

        // PeakHour requires StartTime and EndTime; EndTime must be after StartTime.
        When(x => x.Type == ModifierType.PeakHour, () =>
        {
            RuleFor(x => x.StartTime).NotNull();
            RuleFor(x => x.EndTime).NotNull();
            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
                .WithMessage("EndTime must be after StartTime.");
        });

        // DayOfWeek requires ActiveDayOfWeek.
        When(x => x.Type == ModifierType.DayOfWeek, () =>
        {
            RuleFor(x => x.ActiveDayOfWeek).NotNull();
        });

        // Promotion requires StartDate and EndDate; EndDate >= StartDate.
        When(x => x.Type == ModifierType.Promotion, () =>
        {
            RuleFor(x => x.StartDate).NotNull();
            RuleFor(x => x.EndDate).NotNull();
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("EndDate must be on or after StartDate.");
        });
    }
}
