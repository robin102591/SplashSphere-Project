using FluentValidation;

namespace SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltySettings;

public sealed class UpsertLoyaltySettingsCommandValidator : AbstractValidator<UpsertLoyaltySettingsCommand>
{
    public UpsertLoyaltySettingsCommandValidator()
    {
        RuleFor(x => x.PointsPerCurrencyUnit)
            .GreaterThan(0).WithMessage("Points per currency unit must be greater than 0.");

        RuleFor(x => x.CurrencyUnitAmount)
            .GreaterThan(0).WithMessage("Currency unit amount must be greater than 0.");

        RuleFor(x => x.PointsExpirationMonths)
            .GreaterThan(0).When(x => x.PointsExpirationMonths.HasValue)
            .WithMessage("Expiration months must be greater than 0.");
    }
}
