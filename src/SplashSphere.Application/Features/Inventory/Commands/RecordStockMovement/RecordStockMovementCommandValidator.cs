using FluentValidation;

namespace SplashSphere.Application.Features.Inventory.Commands.RecordStockMovement;

public sealed class RecordStockMovementCommandValidator : AbstractValidator<RecordStockMovementCommand>
{
    public RecordStockMovementCommandValidator()
    {
        RuleFor(x => x.SupplyItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid movement type.");
    }
}
