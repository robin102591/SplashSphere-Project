using FluentValidation;

namespace SplashSphere.Application.Features.Auth.Commands.SetUserPin;

public sealed class SetUserPinCommandValidator : AbstractValidator<SetUserPinCommand>
{
    public SetUserPinCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Pin)
            .NotEmpty()
            .Matches(@"^\d{6}$")
            .WithMessage("PIN must be exactly 6 digits.");
    }
}
