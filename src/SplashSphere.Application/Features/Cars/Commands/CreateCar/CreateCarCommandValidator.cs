using FluentValidation;

namespace SplashSphere.Application.Features.Cars.Commands.CreateCar;

public sealed class CreateCarCommandValidator : AbstractValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty()
            .MaximumLength(20);

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
