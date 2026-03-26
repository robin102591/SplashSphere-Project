using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.BulkApplyAdjustment;

public sealed class BulkApplyAdjustmentCommandValidator : AbstractValidator<BulkApplyAdjustmentCommand>
{
    public BulkApplyAdjustmentCommandValidator()
    {
        RuleFor(x => x.EntryIds)
            .NotEmpty().WithMessage("At least one entry must be selected.")
            .Must(ids => ids.Count <= 200).WithMessage("Cannot apply to more than 200 entries at once.");

        RuleFor(x => x.AdjustmentType).IsInEnum();

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
