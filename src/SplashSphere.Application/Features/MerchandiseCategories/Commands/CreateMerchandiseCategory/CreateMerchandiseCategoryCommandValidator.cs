using FluentValidation;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.CreateMerchandiseCategory;

public sealed class CreateMerchandiseCategoryCommandValidator : AbstractValidator<CreateMerchandiseCategoryCommand>
{
    public CreateMerchandiseCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
