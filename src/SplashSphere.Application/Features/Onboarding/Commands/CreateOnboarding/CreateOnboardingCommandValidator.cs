using FluentValidation;

namespace SplashSphere.Application.Features.Onboarding.Commands.CreateOnboarding;

public sealed class CreateOnboardingCommandValidator : AbstractValidator<CreateOnboardingCommand>
{
    public CreateOnboardingCommandValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Business name is required.")
            .MaximumLength(200);

        RuleFor(x => x.BusinessEmail)
            .NotEmpty().WithMessage("Business email is required.")
            .EmailAddress().WithMessage("Business email must be a valid email address.");

        RuleFor(x => x.ContactNumber)
            .NotEmpty().WithMessage("Contact number is required.")
            .MaximumLength(30);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Business address is required.")
            .MaximumLength(500);

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(200);

        RuleFor(x => x.BranchCode)
            .NotEmpty().WithMessage("Branch code is required.")
            .MaximumLength(10)
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Branch code may only contain letters and numbers.");

        RuleFor(x => x.BranchAddress)
            .NotEmpty().WithMessage("Branch address is required.")
            .MaximumLength(500);

        RuleFor(x => x.BranchContactNumber)
            .NotEmpty().WithMessage("Branch contact number is required.")
            .MaximumLength(30);

        RuleFor(x => x.BusinessType)
            .InclusiveBetween(0, 2)
            .WithMessage("Business type must be Independent (0), Corporate Chain (1), or Franchisor (2).");
    }
}
