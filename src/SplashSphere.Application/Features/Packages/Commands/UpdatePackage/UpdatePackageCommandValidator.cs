using FluentValidation;

namespace SplashSphere.Application.Features.Packages.Commands.UpdatePackage;

public sealed class UpdatePackageCommandValidator : AbstractValidator<UpdatePackageCommand>
{
    public UpdatePackageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.ServiceIds)
            .NotNull()
            .NotEmpty().WithMessage("A package must include at least one service.");
        RuleForEach(x => x.ServiceIds).NotEmpty();
    }
}
