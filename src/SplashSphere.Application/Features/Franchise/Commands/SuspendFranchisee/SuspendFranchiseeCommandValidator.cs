using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.SuspendFranchisee;

public sealed class SuspendFranchiseeCommandValidator : AbstractValidator<SuspendFranchiseeCommand>
{
    public SuspendFranchiseeCommandValidator()
    {
        RuleFor(x => x.FranchiseeTenantId).NotEmpty();
    }
}
