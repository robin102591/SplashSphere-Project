using FluentValidation;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Must(PhoneNumber.IsValid)
            .WithMessage("Phone number must be a valid Philippine mobile number.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches("^[0-9]{4,8}$")
            .WithMessage("Code must be 4-8 digits.");
    }
}
