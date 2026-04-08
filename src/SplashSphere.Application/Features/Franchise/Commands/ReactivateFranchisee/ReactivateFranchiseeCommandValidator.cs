using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.ReactivateFranchisee;

public sealed class ReactivateFranchiseeCommandValidator : AbstractValidator<ReactivateFranchiseeCommand>
{
    public ReactivateFranchiseeCommandValidator()
    {
        RuleFor(x => x.FranchiseeTenantId).NotEmpty();
    }
}
