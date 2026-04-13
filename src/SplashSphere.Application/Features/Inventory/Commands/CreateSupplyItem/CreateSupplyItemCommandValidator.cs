using FluentValidation;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplyItem;

public sealed class CreateSupplyItemCommandValidator : AbstractValidator<CreateSupplyItemCommand>
{
    public CreateSupplyItemCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialUnitCost).GreaterThanOrEqualTo(0);
    }
}
