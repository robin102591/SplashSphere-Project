using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.UpsertServiceTemplate;

public sealed class UpsertServiceTemplateCommandValidator : AbstractValidator<UpsertServiceTemplateCommand>
{
    public UpsertServiceTemplateCommandValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Base price must be zero or greater.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than zero.");
    }
}
