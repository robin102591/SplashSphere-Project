using FluentValidation;

namespace SplashSphere.Application.Features.Connect.Referral.Commands.ApplyReferral;

public sealed class ApplyReferralCommandValidator : AbstractValidator<ApplyReferralCommand>
{
    public ApplyReferralCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(40);
    }
}
