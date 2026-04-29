using FluentValidation;

namespace SplashSphere.Application.Features.PosStations.Commands.CreatePosStation;

public sealed class CreatePosStationCommandValidator : AbstractValidator<CreatePosStationCommand>
{
    public CreatePosStationCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
