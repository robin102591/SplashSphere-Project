using FluentValidation;

namespace SplashSphere.Application.Features.Inventory.Commands.RegisterEquipment;

public sealed class RegisterEquipmentCommandValidator : AbstractValidator<RegisterEquipmentCommand>
{
    public RegisterEquipmentCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
    }
}
