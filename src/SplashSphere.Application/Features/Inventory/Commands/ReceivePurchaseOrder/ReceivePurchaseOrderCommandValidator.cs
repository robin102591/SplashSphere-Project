using FluentValidation;

namespace SplashSphere.Application.Features.Inventory.Commands.ReceivePurchaseOrder;

public sealed class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one receive line is required.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.LineId).NotEmpty();
            line.RuleFor(l => l.ReceivedQuantity).GreaterThan(0)
                .WithMessage("Received quantity must be greater than zero.");
        });
    }
}
