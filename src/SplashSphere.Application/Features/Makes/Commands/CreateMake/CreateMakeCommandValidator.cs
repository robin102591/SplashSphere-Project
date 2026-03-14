using FluentValidation;

namespace SplashSphere.Application.Features.Makes.Commands.CreateMake;

public sealed class CreateMakeCommandValidator : AbstractValidator<CreateMakeCommand>
{
    public CreateMakeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
