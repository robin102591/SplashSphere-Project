using FluentValidation;

namespace SplashSphere.Application.Features.PosStations.Commands.UpdatePosStation;

public sealed class UpdatePosStationCommandValidator : AbstractValidator<UpdatePosStationCommand>
{
    public UpdatePosStationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
