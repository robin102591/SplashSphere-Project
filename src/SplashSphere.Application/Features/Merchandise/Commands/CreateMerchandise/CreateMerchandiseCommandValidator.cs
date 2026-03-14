using FluentValidation;

namespace SplashSphere.Application.Features.Merchandise.Commands.CreateMerchandise;

public sealed class CreateMerchandiseCommandValidator : AbstractValidator<CreateMerchandiseCommand>
{
    public CreateMerchandiseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
