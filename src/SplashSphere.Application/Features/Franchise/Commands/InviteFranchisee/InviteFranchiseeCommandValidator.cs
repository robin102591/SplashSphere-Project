using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.InviteFranchisee;

public sealed class InviteFranchiseeCommandValidator : AbstractValidator<InviteFranchiseeCommand>
{
    public InviteFranchiseeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .MaximumLength(256);
    }
}
