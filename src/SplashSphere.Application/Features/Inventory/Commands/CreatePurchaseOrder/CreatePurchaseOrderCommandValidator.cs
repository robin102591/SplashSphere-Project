using FluentValidation;

namespace SplashSphere.Application.Features.Inventory.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemName).NotEmpty().MaximumLength(256);
            line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit cost must be zero or greater.");
        });
    }
}
