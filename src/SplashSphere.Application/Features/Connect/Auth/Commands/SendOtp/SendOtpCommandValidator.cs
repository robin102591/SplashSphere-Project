using FluentValidation;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.SendOtp;

public sealed class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Must(PhoneNumber.IsValid)
            .WithMessage("Phone number must be a valid Philippine mobile number.");
    }
}
