using FluentValidation;

namespace SplashSphere.Application.Features.Cars.Commands.UpdateCar;

public sealed class UpdateCarCommandValidator : AbstractValidator<UpdateCarCommand>
{
    public UpdateCarCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.VehicleTypeId)
            .NotEmpty();

        RuleFor(x => x.SizeId)
            .NotEmpty();

        RuleFor(x => x.Color)
            .MaximumLength(50);

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .When(x => x.Year.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}
