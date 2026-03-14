using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServiceCommission;

public sealed class UpsertServiceCommissionCommandValidator
    : AbstractValidator<UpsertServiceCommissionCommand>
{
    public UpsertServiceCommissionCommandValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.Rows).NotNull();

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.VehicleTypeId).NotEmpty();
            row.RuleFor(r => r.SizeId).NotEmpty();
            row.RuleFor(r => r.Type).IsInEnum();

            // ── Percentage: PercentageRate required, FixedAmount must be null ─────
            row.When(r => r.Type == CommissionType.Percentage, () =>
            {
                row.RuleFor(r => r.PercentageRate)
                    .NotNull().WithMessage("PercentageRate is required for Percentage commission.")
                    .GreaterThan(0).WithMessage("PercentageRate must be > 0.")
                    .LessThanOrEqualTo(100).WithMessage("PercentageRate must be ≤ 100.")
                    .When(r => r.PercentageRate.HasValue);

                row.RuleFor(r => r.FixedAmount)
                    .Null().WithMessage("FixedAmount must be null for Percentage commission.");
            });

            // ── FixedAmount: FixedAmount required, PercentageRate must be null ────
            row.When(r => r.Type == CommissionType.FixedAmount, () =>
            {
                row.RuleFor(r => r.FixedAmount)
                    .NotNull().WithMessage("FixedAmount is required for FixedAmount commission.")
                    .GreaterThanOrEqualTo(0).WithMessage("FixedAmount must be ≥ 0.")
                    .When(r => r.FixedAmount.HasValue);

                row.RuleFor(r => r.PercentageRate)
                    .Null().WithMessage("PercentageRate must be null for FixedAmount commission.");
            });

            // ── Hybrid: both required ─────────────────────────────────────────────
            row.When(r => r.Type == CommissionType.Hybrid, () =>
            {
                row.RuleFor(r => r.FixedAmount)
                    .NotNull().WithMessage("FixedAmount is required for Hybrid commission.")
                    .GreaterThanOrEqualTo(0).WithMessage("FixedAmount must be ≥ 0.")
                    .When(r => r.FixedAmount.HasValue);

                row.RuleFor(r => r.PercentageRate)
                    .NotNull().WithMessage("PercentageRate is required for Hybrid commission.")
                    .GreaterThan(0).WithMessage("PercentageRate must be > 0.")
                    .LessThanOrEqualTo(100).WithMessage("PercentageRate must be ≤ 100.")
                    .When(r => r.PercentageRate.HasValue);
            });
        });

        // No duplicate (VehicleTypeId, SizeId) pairs.
        RuleFor(x => x.Rows)
            .Must(rows => rows
                .GroupBy(r => (r.VehicleTypeId, r.SizeId))
                .All(g => g.Count() == 1))
            .WithMessage("Commission rows contain duplicate (VehicleTypeId, SizeId) combinations.")
            .When(x => x.Rows is { Count: > 0 });
    }
}
