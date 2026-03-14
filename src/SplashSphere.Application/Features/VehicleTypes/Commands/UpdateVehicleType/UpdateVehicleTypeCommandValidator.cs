using FluentValidation;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.UpdateVehicleType;

public sealed class UpdateVehicleTypeCommandValidator : AbstractValidator<UpdateVehicleTypeCommand>
{
    public UpdateVehicleTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
