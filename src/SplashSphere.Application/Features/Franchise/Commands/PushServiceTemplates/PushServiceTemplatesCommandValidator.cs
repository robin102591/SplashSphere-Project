using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.PushServiceTemplates;

public sealed class PushServiceTemplatesCommandValidator : AbstractValidator<PushServiceTemplatesCommand>
{
    public PushServiceTemplatesCommandValidator()
    {
        RuleFor(x => x.FranchiseeTenantId).NotEmpty();
    }
}
