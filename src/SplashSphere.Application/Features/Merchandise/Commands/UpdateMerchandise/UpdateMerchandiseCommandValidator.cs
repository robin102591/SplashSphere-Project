using FluentValidation;

namespace SplashSphere.Application.Features.Merchandise.Commands.UpdateMerchandise;

public sealed class UpdateMerchandiseCommandValidator : AbstractValidator<UpdateMerchandiseCommand>
{
    public UpdateMerchandiseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CostPrice.HasValue);

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
