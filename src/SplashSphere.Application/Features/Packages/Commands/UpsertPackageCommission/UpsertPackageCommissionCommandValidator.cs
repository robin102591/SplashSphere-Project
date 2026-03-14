using FluentValidation;

namespace SplashSphere.Application.Features.Packages.Commands.UpsertPackageCommission;

public sealed class UpsertPackageCommissionCommandValidator : AbstractValidator<UpsertPackageCommissionCommand>
{
    public UpsertPackageCommissionCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Rows).NotNull();

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.VehicleTypeId).NotEmpty();
            row.RuleFor(r => r.SizeId).NotEmpty();
            row.RuleFor(r => r.PercentageRate)
                .GreaterThan(0).WithMessage("PercentageRate must be > 0.")
                .LessThanOrEqualTo(100).WithMessage("PercentageRate must be ≤ 100.");
        });

        RuleFor(x => x.Rows)
            .Must(rows => rows.GroupBy(r => (r.VehicleTypeId, r.SizeId)).All(g => g.Count() == 1))
            .WithMessage("Commission rows contain duplicate (VehicleTypeId, SizeId) combinations.")
            .When(x => x.Rows is { Count: > 0 });
    }
}
