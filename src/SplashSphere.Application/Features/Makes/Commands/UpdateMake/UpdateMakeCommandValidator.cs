using FluentValidation;

namespace SplashSphere.Application.Features.Makes.Commands.UpdateMake;

public sealed class UpdateMakeCommandValidator : AbstractValidator<UpdateMakeCommand>
{
    public UpdateMakeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
