using FluentValidation;

namespace SplashSphere.Application.Features.Sizes.Commands.UpdateSize;

public sealed class UpdateSizeCommandValidator : AbstractValidator<UpdateSizeCommand>
{
    public UpdateSizeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
