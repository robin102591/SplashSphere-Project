using FluentValidation;

namespace SplashSphere.Application.Features.Models.Commands.UpdateModel;

public sealed class UpdateModelCommandValidator : AbstractValidator<UpdateModelCommand>
{
    public UpdateModelCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
