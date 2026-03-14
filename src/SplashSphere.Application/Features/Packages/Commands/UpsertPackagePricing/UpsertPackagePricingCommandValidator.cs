using FluentValidation;

namespace SplashSphere.Application.Features.Packages.Commands.UpsertPackagePricing;

public sealed class UpsertPackagePricingCommandValidator : AbstractValidator<UpsertPackagePricingCommand>
{
    public UpsertPackagePricingCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Rows).NotNull();

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.VehicleTypeId).NotEmpty();
            row.RuleFor(r => r.SizeId).NotEmpty();
            row.RuleFor(r => r.Price).GreaterThanOrEqualTo(0);
        });

        RuleFor(x => x.Rows)
            .Must(rows => rows.GroupBy(r => (r.VehicleTypeId, r.SizeId)).All(g => g.Count() == 1))
            .WithMessage("Pricing rows contain duplicate (VehicleTypeId, SizeId) combinations.")
            .When(x => x.Rows is { Count: > 0 });
    }
}
