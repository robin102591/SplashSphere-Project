using FluentValidation;

namespace SplashSphere.Application.Features.Sizes.Commands.CreateSize;

public sealed class CreateSizeCommandValidator : AbstractValidator<CreateSizeCommand>
{
    public CreateSizeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
