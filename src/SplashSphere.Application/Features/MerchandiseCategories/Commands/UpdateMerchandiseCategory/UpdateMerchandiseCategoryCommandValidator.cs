using FluentValidation;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.UpdateMerchandiseCategory;

public sealed class UpdateMerchandiseCategoryCommandValidator : AbstractValidator<UpdateMerchandiseCategoryCommand>
{
    public UpdateMerchandiseCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
