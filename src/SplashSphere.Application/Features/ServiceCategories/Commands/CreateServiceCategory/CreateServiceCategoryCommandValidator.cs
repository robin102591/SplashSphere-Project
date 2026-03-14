using FluentValidation;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.CreateServiceCategory;

public sealed class CreateServiceCategoryCommandValidator : AbstractValidator<CreateServiceCategoryCommand>
{
    public CreateServiceCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
