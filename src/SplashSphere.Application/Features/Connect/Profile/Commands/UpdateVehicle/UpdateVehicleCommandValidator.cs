using FluentValidation;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.UpdateVehicle;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.MakeId).NotEmpty();
        RuleFor(x => x.ModelId).NotEmpty();

        RuleFor(x => x.PlateNumber)
            .NotEmpty()
            .MaximumLength(16);

        RuleFor(x => x.Color)
            .MaximumLength(32);

        RuleFor(x => x.Year)
            .InclusiveBetween(1950, DateTime.UtcNow.Year + 1)
            .When(x => x.Year.HasValue);
    }
}
