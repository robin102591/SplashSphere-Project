using FluentValidation;
using SplashSphere.Application.Features.Inventory.Commands.CreatePurchaseOrder;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemName).NotEmpty().MaximumLength(256);
            line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit cost must be zero or greater.");
        });
    }
}
