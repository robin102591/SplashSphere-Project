using FluentValidation;

namespace SplashSphere.Application.Features.Merchandise.Commands.AdjustStock;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.MerchandiseId).NotEmpty();

        RuleFor(x => x.Adjustment)
            .NotEqual(0).WithMessage("Adjustment must be non-zero.");

        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}
