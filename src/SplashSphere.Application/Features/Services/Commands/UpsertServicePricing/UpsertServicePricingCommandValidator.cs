using FluentValidation;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServicePricing;

public sealed class UpsertServicePricingCommandValidator : AbstractValidator<UpsertServicePricingCommand>
{
    public UpsertServicePricingCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.Rows).NotNull();

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.VehicleTypeId).NotEmpty();
            row.RuleFor(r => r.SizeId).NotEmpty();
            row.RuleFor(r => r.Price).GreaterThanOrEqualTo(0);
        });

        // No duplicate (VehicleTypeId, SizeId) pairs in a single submission.
        RuleFor(x => x.Rows)
            .Must(rows => rows
                .GroupBy(r => (r.VehicleTypeId, r.SizeId))
                .All(g => g.Count() == 1))
            .WithMessage("Pricing rows contain duplicate (VehicleTypeId, SizeId) combinations.")
            .When(x => x.Rows is { Count: > 0 });
    }
}
