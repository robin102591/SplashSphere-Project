using FluentValidation;

namespace SplashSphere.Application.Features.Models.Commands.CreateModel;

public sealed class CreateModelCommandValidator : AbstractValidator<CreateModelCommand>
{
    public CreateModelCommandValidator()
    {
        RuleFor(x => x.MakeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
