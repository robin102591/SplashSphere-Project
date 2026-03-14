using FluentValidation;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.CreateVehicleType;

public sealed class CreateVehicleTypeCommandValidator : AbstractValidator<CreateVehicleTypeCommand>
{
    public CreateVehicleTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
